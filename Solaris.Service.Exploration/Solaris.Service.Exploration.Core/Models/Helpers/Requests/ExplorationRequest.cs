using System.Collections.Generic;
using Solaris.Service.Exploration.Core.Models.Entities;

namespace Solaris.Service.Exploration.Core.Models.Helpers.Requests
{
    public class ExplorationRequest
    {
        public Planet Planet { get; set; }
        public List<Robot> Robots { get; set; }
    }
}