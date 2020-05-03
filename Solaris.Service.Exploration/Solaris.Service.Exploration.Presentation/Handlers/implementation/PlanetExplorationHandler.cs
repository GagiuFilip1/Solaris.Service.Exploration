using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Solaris.Service.Exploration.Core.Enums;
using Solaris.Service.Exploration.Core.Handlers;
using Solaris.Service.Exploration.Core.Handlers.Interfaces;
using Solaris.Service.Exploration.Core.Models.Entities;
using Solaris.Service.Exploration.Core.Models.Helpers.Commons;
using Solaris.Service.Exploration.Core.Models.Requests;
using Solaris.Service.Exploration.Core.Services;
using Solaris.Service.Exploration.Core.Services.Interfaces;
using Solaris.Service.Exploration.Infrastructure.Rabbit;

namespace Solaris.Service.Exploration.Presentation.Handlers.implementation
{
    public class PlanetExplorationHandler : IPlanetExplorationHandler
    {
        private readonly RabbitHandler m_rabbitHandler;
        private readonly IExplorationService m_explorationService;
        private readonly AppSettings m_appSettings;
        private readonly ILogger<PlanetExplorationHandler> m_logger;

        public PlanetExplorationHandler(RabbitHandler rabbitHandler, IOptions<AppSettings> appSettings, IExplorationService explorationService)
        {
            m_appSettings = appSettings.Value;
            m_rabbitHandler = rabbitHandler;
            m_explorationService = explorationService;
        }

        public void HandleAsync()
        {
            m_rabbitHandler.ListenQueueAsync(new ListenOptions
            {
                MessageType = MessageType.SendRobotsToPlanet,
                RequestParser = ExploreAsync,
                TargetQueue = m_appSettings.RabbitMqQueues.ExplorationQueue
            });
        }

        private async Task ExploreAsync(string body)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<ExplorationRequest>(body);
                var response = new ExplorationResponse();
                try
                {
                    await m_explorationService.ExplorePlanet(response);
                }
                catch (ValidationException e)
                {
                    m_logger.LogError(e, "Validation Error Occured while trying to explore the planet");
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
                if (t.Status != RobotStatus.Broken)
                    t.Status = RobotStatus.Free;
            });
        }

        private void SendPlanetExplorationResult(ExplorationResponse response)
        {
            m_rabbitHandler.PublishRpc(new PublishOptions
            {
                TargetQueue = m_appSettings.RabbitMqQueues.SolarApiQueue,
                Headers = new Dictionary<string, object>
                {
                    {nameof(MessageType), MessageType.ExplorationFinished},
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
                    {nameof(MessageType), MessageType.UpdateRobotStatus},
                },
                Message = JsonConvert.SerializeObject(robots)
            });
        }
    }
}