using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Solaris.Service.Exploration.Infrastructure.Rabbit
{
    public class RpcData : IDisposable
    {
        public IBasicProperties BasicProperties { get; private set; }
        public EventingBasicConsumer Consumer { get; private set; }
        public IModel Channel { get; private set; }
        private IConnection Connection { get; set; }
        public string ReplyQueueName { get; private set; }
        public readonly BlockingCollection<string> ResponseQueue = new BlockingCollection<string>();

        public RpcData(IConnectionFactory factory, IDictionary<string, object> headers)
        {
            ResolveData(factory, headers);
        }

        private void ResolveData(IConnectionFactory factory, IDictionary<string, object> headers)
        {
            var rpcIdentity = Guid.NewGuid().ToString();
            Connection = factory.CreateConnection();
            Channel = Connection.CreateModel();
            Consumer = new EventingBasicConsumer(Channel);
            ReplyQueueName = Channel.QueueDeclare().QueueName;
            BuildBasicProperties(Channel, rpcIdentity, headers);
            Consumer.Received += (model, args) =>
            {
                var body = args.Body;
                var response = Encoding.UTF8.GetString(body.ToArray());
                if (args.BasicProperties.CorrelationId == rpcIdentity)
                    ResponseQueue.Add(response);
            };
        }

        private void BuildBasicProperties(IModel channel, string rpcIdentity, IDictionary<string, object> headers)
        {
            BasicProperties = channel.CreateBasicProperties();
            BasicProperties.CorrelationId = rpcIdentity;
            BasicProperties.ReplyTo = ReplyQueueName;
            BasicProperties.Headers = headers;
        }

        public void Dispose()
        {
            Channel?.Dispose();
            Connection?.Dispose();
        }
    }
}