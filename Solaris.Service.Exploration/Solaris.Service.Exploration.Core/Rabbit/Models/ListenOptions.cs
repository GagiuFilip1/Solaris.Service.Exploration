using System;
using System.Threading.Tasks;
using Solaris.Service.Exploration.Core.Enums;

namespace Solaris.Service.Exploration.Core.Rabbit.Models
{
    public class ListenOptions
    {
        public string TargetQueue { get; set; }
        public Func<string, Task> RequestParser { get; set; }
        public MessageType MessageType { get; set; }
        public ushort Qos { get; set; }
    }
}