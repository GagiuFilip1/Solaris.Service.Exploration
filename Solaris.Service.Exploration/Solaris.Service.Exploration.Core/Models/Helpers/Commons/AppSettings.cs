using Solaris.Service.Exploration.Core.Rabbit.Helpers.Setup;

namespace Solaris.Service.Exploration.Core.Models.Helpers.Commons
{
    public class AppSettings
    {
        public RabbitMqSettings RabbitMq { get; set; }

        public RabbitMqQueues RabbitMqQueues { get; set; }
    }
}