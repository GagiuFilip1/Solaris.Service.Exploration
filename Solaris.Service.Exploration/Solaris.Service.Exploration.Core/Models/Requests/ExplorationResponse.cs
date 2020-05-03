using System.Collections.Generic;
using Solaris.Service.Exploration.Core.Models.Entities;

namespace Solaris.Service.Exploration.Core.Models.Requests
{
    public class ExplorationResponse
    {
        public bool IsSuccessful { get; set; }
        public Planet Planet { get; set; }
        public List<Robot> Robots { get; set; }
    }
}