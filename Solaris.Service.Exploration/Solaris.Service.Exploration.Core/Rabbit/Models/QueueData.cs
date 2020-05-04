using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Solaris.Service.Exploration.Core.Rabbit.Models
{
    public class QueueData : IDisposable
    {
        public IBasicProperties BasicProperties { get; }
        public IModel Channel { get; }
        private IConnection Connection { get; }
        public EventingBasicConsumer Consumer { get; }
        
        public QueueData(IConnectionFactory factory, IDictionary<string, object> headers)
        {
            Connection = factory.CreateConnection();
            Channel = Connection.CreateModel();
            BasicProperties = Channel.CreateBasicProperties();
            BasicProperties.Headers = headers;
            Consumer = new EventingBasicConsumer(Channel);
        }

        public void Dispose()
        {
            Channel?.Dispose();
            Connection?.Dispose();
        }
    }
}