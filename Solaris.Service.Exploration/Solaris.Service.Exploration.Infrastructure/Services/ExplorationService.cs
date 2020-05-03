using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Solaris.Service.Exploration.Core.Enums;
using Solaris.Service.Exploration.Core.Extensions;
using Solaris.Service.Exploration.Core.Models.Entities;
using Solaris.Service.Exploration.Core.Models.Requests;
using Solaris.Service.Exploration.Core.Services.Interfaces;
using Solaris.Service.Exploration.Infrastructure.Ioc;

namespace Solaris.Service.Exploration.Infrastructure.Services
{
    [RegistrationKind(Type = RegistrationType.Scoped, AsSelf = false)]
    public class ExplorationService : IExplorationService
    {
        private readonly Random m_random = new Random();
        private const float GET_TEMPERATURE_MALFUNCTION_FACTOR = 0.1F;
        private const float GET_WATER_PERCENTAGE_MALFUNCTION_FACTOR = 0.01F;
        private const float GET_OXYGEN_PERCENTAGE_MALFUNCTION_FACTOR = 0.01F;
        private const float GET_GRAVITY_FORCE_MALFUNCTION_FACTOR = 0.001F;
        private const float GET_MAGNETIC_FIELD_FACTOR = 0.001F;
        private const float GET_PLANET_RADIUS_MALFUNCTION_FACTOR = 0.0001F;
        private const float GET_PLANET_SPIN_FACTOR = 0.0001F;
        private const float GET_PLANET_SOLAR_WIND_FACTOR = 0.01F;
        private const float ROBOT_SPEED_FACTOR = 0.1F;

        public async Task ExplorePlanet(ExplorationResponse response)
        {
            var planet = response.Planet;
            var robots = response.Robots;
            //one robot will complete a task in one second, and each other robot will speed up the process with 0.01% with a min waiting time of 0.5 seconds
            await GetPlanetSpinFrequency(planet, robots);
            await GetPlanetAverageSolarWindVelocity(planet, robots);
            await GetPlanetRadius(planet, robots);
            await GetPlanetTemperature(planet, robots);
            await GetPlanetWaterPercentage(planet, robots);
            await GetPlanetOxygenPercentage(planet, robots);
            await GetPlanetGravityForce(planet, robots);
            await GetPlanetSurfaceMagneticField(planet, robots);
            
            SumarizePlanet(planet);
        }

        private static void SumarizePlanet(Planet planet)
        {
            if (planet.TemperatureDay > 60 || planet.TemperatureNight < 60)
            {
                planet.PlanetStatus = PlanetStatus.Uninhabitable;
                return;
            }

            if (planet.WaterPercentage < 10)
            {
                planet.PlanetStatus = PlanetStatus.Uninhabitable;
                return;
            }

            if (planet.OxygenPercentage < 18)
            {
                planet.PlanetStatus = PlanetStatus.Uninhabitable;
                return;
            }

            if (planet.SpinFrequency < 6)
            {
                planet.PlanetStatus = PlanetStatus.Uninhabitable;
                return;
            }

            if (planet.PlanetRadius < 1000)
            {
                planet.PlanetStatus = PlanetStatus.Uninhabitable;
                return;
            }
            
            if (planet.PlanetSurfaceMagneticField < 15 || planet.PlanetSurfaceMagneticField > 100)
            {
                planet.PlanetStatus = PlanetStatus.Uninhabitable;
                return;
            }

            if (planet.GravityForce > 20)
            {
                planet.PlanetStatus = PlanetStatus.Uninhabitable;
                return;
            }

            if (planet.AverageSolarWindVelocity > 700)
            {
                planet.PlanetStatus = PlanetStatus.Uninhabitable;
                return;
            }

            planet.PlanetStatus = PlanetStatus.Habitable;
        }

        private async Task GetPlanetTemperature(Planet planet, List<Robot> robots)
        {
            planet.TemperatureDay = m_random.NextFloat(-273.15F, 5505F);
            planet.TemperatureNight = m_random.NextFloat(-273.15F, 5505F);
            robots.ForEach(t => t.ApplyRobotMalfunction(GET_TEMPERATURE_MALFUNCTION_FACTOR));
            await Task.Delay(GetOperationDuration(robots));
        }

        private async Task GetPlanetWaterPercentage(Planet planet, List<Robot> robots)
        {
            planet.WaterPercentage = m_random.NextFloat(0.0F, 100.0F);
            robots.ForEach(t => t.ApplyRobotMalfunction(GET_WATER_PERCENTAGE_MALFUNCTION_FACTOR));
            await Task.Delay(GetOperationDuration(robots));
        }

        private async Task GetPlanetOxygenPercentage(Planet planet, List<Robot> robots)
        {
            planet.OxygenPercentage = m_random.NextFloat(0.0F, 100.0F);
            robots.ForEach(t => t.ApplyRobotMalfunction(GET_OXYGEN_PERCENTAGE_MALFUNCTION_FACTOR));
            await Task.Delay(GetOperationDuration(robots));
        }

        private async Task GetPlanetGravityForce(Planet planet, List<Robot> robots)
        {
            planet.GravityForce = m_random.NextFloat(0.01F, 1000.0F);
            robots.ForEach(t => t.ApplyRobotMalfunction(GET_GRAVITY_FORCE_MALFUNCTION_FACTOR * planet.GravityForce));
            await Task.Delay(GetOperationDuration(robots));
        }

        private async Task GetPlanetRadius(Planet planet, List<Robot> robots)
        {
            planet.PlanetRadius = m_random.NextFloat(500, 100000.0F);
            robots.ForEach(t => t.ApplyRobotMalfunction(GET_PLANET_RADIUS_MALFUNCTION_FACTOR));
            await Task.Delay(GetOperationDuration(robots));
        }

        private async Task GetPlanetSurfaceMagneticField(Planet planet, List<Robot> robots)
        {
            planet.PlanetSurfaceMagneticField = m_random.NextFloat(0.01F, 1000.0F);
            robots.ForEach(t => t.ApplyRobotMalfunction(GET_MAGNETIC_FIELD_FACTOR * planet.PlanetSurfaceMagneticField));
            await Task.Delay(GetOperationDuration(robots));
        }

        private async Task GetPlanetSpinFrequency(Planet planet, List<Robot> robots)
        {
            planet.SpinFrequency = m_random.NextFloat(0.001F, 1000F);
            robots.ForEach(t => t.ApplyRobotMalfunction(GET_PLANET_SPIN_FACTOR));
            await Task.Delay(GetOperationDuration(robots));
        }

        private async Task GetPlanetAverageSolarWindVelocity(Planet planet, List<Robot> robots)
        {
            planet.AverageSolarWindVelocity = m_random.NextFloat(1.0F, 10000F);
            robots.ForEach(t => t.ApplyRobotMalfunction(GET_PLANET_SOLAR_WIND_FACTOR));
            await Task.Delay(GetOperationDuration(robots));
        }

        private static int GetOperationDuration(IEnumerable<Robot> robots)
        {
            var workingRobots = robots.Count(t => !t.Status.Equals(RobotStatus.Broken));
            if(workingRobots == 0)
                throw new ValidationException("There are no more working robots on the planet");
            var value = (int) Math.Ceiling(1000 - workingRobots * ROBOT_SPEED_FACTOR);
            return value > 500 ? 500 : value;
        }
    }
}