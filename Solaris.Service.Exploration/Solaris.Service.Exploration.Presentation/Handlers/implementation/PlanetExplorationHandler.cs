using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Solaris.Service.Exploration.Core.Enums;
using Solaris.Service.Exploration.Core.Handlers.Interfaces;
using Solaris.Service.Exploration.Core.Models.Entities;
using Solaris.Service.Exploration.Core.Models.Helpers.Commons;
using Solaris.Service.Exploration.Core.Models.Helpers.Requests;
using Solaris.Service.Exploration.Core.Rabbit.Interfaces;
using Solaris.Service.Exploration.Core.Rabbit.Models;
using Solaris.Service.Exploration.Core.Services.Interfaces;
using Solaris.Service.Exploration.Infrastructure.Ioc;

namespace Solaris.Service.Exploration.Presentation.Handlers.implementation
{
    [RegistrationKind(Type = RegistrationType.Scoped, AsSelf = false, SpecificInterface = typeof(IHandler))]
    public class PlanetExplorationHandler : IPlanetExplorationHandler
    {
        private readonly IRabbitHandler m_rabbitHandler;
        private readonly IExplorationService m_explorationService;
        private readonly AppSettings m_appSettings;
        private readonly ILogger<PlanetExplorationHandler> m_logger;

        public PlanetExplorationHandler(IRabbitHandler rabbitHandler, IOptions<AppSettings> appSettings, IExplorationService explorationService, ILogger<PlanetExplorationHandler> logger)
        {
            m_appSettings = appSettings.Value;
            m_rabbitHandler = rabbitHandler;
            m_explorationService = explorationService;
            m_logger = logger;
        }

        public void HandleAsync()
        {
            m_rabbitHandler.ListenQueueAsync(new ListenOptions
            {
                MessageType = MessageType.StartExplorationProcess,
                RequestParser = ExploreAsync,
                TargetQueue = m_appSettings.RabbitMqQueues.ExplorationQueue,
                Qos = 10
            });
        }

        private async Task ExploreAsync(string body)
        {
            try
            {
                m_logger.LogInformation($"Received Job {body}");
                var request = JsonConvert.DeserializeObject<ExplorationRequest>(body);
                var response = new ExplorationResponse
                {
                    Robots = request.Robots,
                    Planet = request.Planet
                };
                try
                {
                    await m_explorationService.ExplorePlanet(response);
                }
                catch (ValidationException e)
                {
                    m_logger.LogWarning(e, "Validation Error Occured while trying to explore the planet");
                }
                catch (Exception e) when (e.GetType() != typeof(ValidationException))
                {
                    m_logger.LogCritical("Unexpected Error Occured while trying to explore the planet");
                }
                finally
                {
                    SendPlanetExplorationResult(response);
                    SetRobotsStatus(request.Robots);
                    SendRobotsStatus(request.Robots);
                }
            }
            catch
            {
                m_logger.LogCritical($"Could not deserialize the request body : {body}");
            }
        }

        private static void SetRobotsStatus(List<Robot> robots)
        {
            robots.ForEach(t =>
            {
                if (t.CurrentStatus != RobotStatus.Broken)
                    t.CurrentStatus = RobotStatus.Free;
            });
        }

        private void SendPlanetExplorationResult(ExplorationResponse response)
        {
            m_rabbitHandler.PublishRpc(new PublishOptions
            {
                TargetQueue = m_appSettings.RabbitMqQueues.SolarApiQueue,
                Headers = new Dictionary<string, object>
                {
                    {nameof(MessageType), nameof(MessageType.ExplorationFinished)},
                },
                Message = JsonConvert.SerializeObject(response)
            });
        }

        private void SendRobotsStatus(List<Robot> robots)
        {
            m_rabbitHandler.PublishRpc(new PublishOptions
            {
                TargetQueue = m_appSettings.RabbitMqQueues.CrewApiQueue,
                Headers = new Dictionary<string, object>
                {
                    {nameof(MessageType), nameof(MessageType.UpdateRobotStatus)},
                },
                Message = JsonConvert.SerializeObject(new {robots})
            });
        }
    }
}