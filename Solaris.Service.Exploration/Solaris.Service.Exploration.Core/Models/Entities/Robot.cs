using System;
using Solaris.Service.Exploration.Core.Enums;

namespace Solaris.Service.Exploration.Core.Models.Entities
{
    public class Robot
    {
        public Guid Id { get; set; }
        public RobotStatus Status { get; set; }
        public void ApplyRobotMalfunction(float malfunctionFactor)
        {
            var random = new Random();
            if (random.NextDouble() < malfunctionFactor) Status = RobotStatus.Broken;
        }
    }
}