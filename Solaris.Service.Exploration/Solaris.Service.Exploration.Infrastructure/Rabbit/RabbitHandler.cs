using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Solaris.Service.Exploration.Core.Enums;
using Solaris.Service.Exploration.Core.Models.Helpers.Commons;
using Solaris.Service.Exploration.Infrastructure.Ioc;

namespace Solaris.Service.Exploration.Infrastructure.Rabbit
{
    [RegistrationKind(Type = RegistrationType.Singleton, AsSelf = true)]
    public class RabbitHandler
    {
        private readonly AppSettings m_appSettings;
        private ConnectionFactory Factory { get; set; }

        public RabbitHandler()
        {
        }

        public RabbitHandler(IOptions<AppSettings> appSettings)
        {
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
            InitialiseQueue(m_appSettings.RabbitMqQueues.ExplorationQueue, 10, out _);
            InitialiseQueue(m_appSettings.RabbitMqQueues.CrewApiQueue, 10, out _);
            InitialiseQueue(m_appSettings.RabbitMqQueues.SolarApiQueue, 10, out _);
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
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            };
        }
    }
}