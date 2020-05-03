using System;
using Solaris.Service.Exploration.Core.Enums;

namespace Solaris.Service.Exploration.Core.Models.Entities
{
    public class Planet
    {
        public Guid Id { get; set; }
        
        public float TemperatureNight { get; set; }

        public float TemperatureDay { get; set; }

        public float WaterPercentage { get; set; }

        public float OxygenPercentage { get; set; }

        public float GravityForce { get; set; }

        public float PlanetRadius { get; set; }

        public float PlanetSurfaceMagneticField { get; set; }

        public float AverageSolarWindVelocity { get; set; }

        public float SpinFrequency { get; set; }

        public PlanetStatus PlanetStatus { get; set; }
    }
}