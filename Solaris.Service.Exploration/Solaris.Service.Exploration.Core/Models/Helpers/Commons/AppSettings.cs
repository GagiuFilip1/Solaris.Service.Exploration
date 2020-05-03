using Solaris.Service.Exploration.Core.Models.Helpers.Rabbit;

namespace Solaris.Service.Exploration.Core.Models.Helpers.Commons
{
    public class AppSettings
    {
        public RabbitMqSettings RabbitMq { get; set; }

        public RabbitMqQueues RabbitMqQueues { get; set; }
    }
}