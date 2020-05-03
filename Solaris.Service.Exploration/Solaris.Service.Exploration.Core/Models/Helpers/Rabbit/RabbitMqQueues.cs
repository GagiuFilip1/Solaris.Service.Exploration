namespace Solaris.Service.Exploration.Core.Models.Helpers.Rabbit
{
    public class RabbitMqQueues
    {
        public string ExplorationQueue { get; set; }
        public string SolarApiQueue { get; set; }
        public string CrewApiQueue { get; set; }
    }
}