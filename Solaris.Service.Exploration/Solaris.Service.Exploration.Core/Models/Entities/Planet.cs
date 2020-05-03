using System;
using Solaris.Service.Exploration.Core.Enums;

namespace Solaris.Service.Exploration.Core.Models.Entities
{
    public class Planet
    {
        public Guid Id { get; set; }
        public PlanetStatus Status { get; set; }
    }
}