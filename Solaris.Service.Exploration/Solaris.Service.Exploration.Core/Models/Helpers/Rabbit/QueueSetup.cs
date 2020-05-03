namespace Solaris.Service.Exploration.Core.Models.Helpers.Rabbit
{
    public class QueueSetup
    {
        public string QueueName { get; set; }
        public ushort Qos { get; set; }
    }
}