using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Solaris.Service.Exploration.Core.Enums;
using Solaris.Service.Exploration.Core.Models.Helpers.Commons;
using Solaris.Service.Exploration.Core.Models.Helpers.Rabbit;
using Solaris.Service.Exploration.Infrastructure.Ioc;

namespace Solaris.Service.Exploration.Infrastructure.Rabbit
{
    [RegistrationKind(Type = RegistrationType.Singleton, AsSelf = true)]
    public class RabbitHandler
    {
  private readonly AppSettings m_appSettings;
        private readonly ILogger<RabbitHandler> m_logger;
        private ConnectionFactory Factory { get; set; }
        public Dictionary<MessageType, Func<string, Task<RabbitResponse>>> Processors { get; } = new Dictionary<MessageType, Func<string, Task<RabbitResponse>>>();

        public RabbitHandler(IOptions<AppSettings> appSettings, ILogger<RabbitHandler> logger)
        {
            m_logger = logger;
            m_appSettings = appSettings.Value;
            Initialize();
        }

        private void Initialize()
        {
            Factory = new ConnectionFactory
            {
                HostName = m_appSettings.RabbitMq.Host,
                Port = m_appSettings.RabbitMq.Port,
                UserName = m_appSettings.RabbitMq.Username,
                Password = m_appSettings.RabbitMq.Password
            };
        }

        public T PublishRpc<T>(PublishOptions options)
        {
            using var rpcData = new RpcData(Factory, options.Headers);
            rpcData.Channel.BasicPublish(
                "",
                options.TargetQueue,
                rpcData.BasicProperties,
                Encoding.UTF8.GetBytes(options.Message));

            rpcData.Channel.BasicConsume(rpcData.Consumer, rpcData.ReplyQueueName, true);
            var received = rpcData.ResponseQueue.Take();

            return JsonConvert.DeserializeObject<T>(received);
        }

        public void PublishRpc(PublishOptions options)
        {
            using var rpcData = new RpcData(Factory, options.Headers);
            rpcData.Channel.BasicPublish(
                "",
                options.TargetQueue,
                rpcData.BasicProperties,
                Encoding.UTF8.GetBytes(options.Message));
        }

        public void Publish(PublishOptions options)
        {
            using var queueData = new QueueData(Factory, options.Headers);
            queueData.Channel.BasicPublish(
                "",
                options.TargetQueue,
                queueData.BasicProperties,
                Encoding.UTF8.GetBytes(options.Message));
        }

        public void ListenQueueAsync(ListenOptions options)
        {
            var queueData = new QueueData(Factory, null);
            queueData.Channel.BasicQos(0, options.Qos, false);
            queueData.Channel.BasicConsume(options.TargetQueue, false, queueData.Consumer);
            queueData.Consumer.Received += async (model, eventArgs) =>
            {
                var headers = eventArgs.BasicProperties.Headers.ToDictionary(
                    t => t.Key,
                    t => Encoding.UTF8.GetString(t.Value as byte[]));
                try
                {
                    Enum.TryParse(headers[nameof(MessageType)], out MessageType type);
                    if (!type.Equals(options.MessageType))
                        return;
                    var body = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                    await options.RequestParser.Invoke(body);
                    queueData.Channel.BasicAck(eventArgs.DeliveryTag, false);
                    queueData.Dispose();
                    ListenQueueAsync(options);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            };
        }

        public void DeclareRpcQueue(QueueSetup setup)
        {
            InitialiseRpcQueue(setup.QueueName, setup.Qos, out var consumer, out var channel);
            consumer.Received += async (model, eventArgs) =>
            {
                try
                {
                    var headers = eventArgs.BasicProperties.Headers.ToDictionary(
                        t => t.Key,
                        t => Encoding.UTF8.GetString(t.Value as byte[]));
                    var body = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                    var basicProperties = eventArgs.BasicProperties;
                    var properties = channel.CreateBasicProperties();
                    properties.CorrelationId = basicProperties.CorrelationId;

                    try
                    {
                        Enum.TryParse(headers[nameof(MessageType)], out MessageType type);
                        var data = await Processors[type].Invoke(body);
                        PublishAndAcknowledge(channel, basicProperties, properties, data, eventArgs);
                    }
                    catch (Exception e)
                    {
                        m_logger.LogError(e, $"Could not finish a remote request : {JsonConvert.SerializeObject(headers)}");
                        PublishAndAcknowledge(channel, basicProperties, properties, new RabbitResponse(), eventArgs);
                    }
                }
                catch (Exception e)
                {
                    m_logger.LogCritical(e, $"Could not extract request information for {setup.QueueName}");
                }
            };
        }
        
        public void DeclareQueue(QueueSetup queueSetup)
        {
            InitialiseQueue(queueSetup.QueueName, queueSetup.Qos, out _);
        }

        private void InitialiseRpcQueue(string queue, ushort qos, out EventingBasicConsumer consumer, out IModel channel)
        {
            var factory = new ConnectionFactory
            {
                HostName = m_appSettings.RabbitMq.Host,
                Port = m_appSettings.RabbitMq.Port,
                UserName = m_appSettings.RabbitMq.Username,
                Password = m_appSettings.RabbitMq.Password
            };
            var connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(queue, false, false, false, null);
            channel.BasicQos(0, qos, false);
            consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue, false, consumer);
        }
        
        private void InitialiseQueue(string queue, ushort qos, out IModel channel)
        {
            var factory = new ConnectionFactory
            {
                HostName = m_appSettings.RabbitMq.Host,
                Port = m_appSettings.RabbitMq.Port,
                UserName = m_appSettings.RabbitMq.Username,
                Password = m_appSettings.RabbitMq.Password
            };
            var connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(queue, false, false, false, null);
            channel.BasicQos(0, qos, false);
        }


        private static void PublishAndAcknowledge(IModel channel, IBasicProperties basicProperties, IBasicProperties properties, RabbitResponse data, BasicDeliverEventArgs eventArgs)
        {
            if (data != null)
                channel.BasicPublish(string.Empty, basicProperties.ReplyTo, properties, CreateResponse(data));
            channel.BasicAck(eventArgs.DeliveryTag, false);
        }

        private static byte[] CreateResponse(RabbitResponse data)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        }
    }
}