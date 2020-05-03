using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Solaris.Service.Exploration.Core.Enums;
using Solaris.Service.Exploration.Core.Handlers;
using Solaris.Service.Exploration.Core.Models.Helpers.Commons;
using Solaris.Service.Exploration.Core.Models.Requests;
using Solaris.Service.Exploration.Infrastructure.Rabbit;

namespace Solaris.Service.Exploration.Presentation.Handlers
{
    public class PlanetExplorationHandler : IPlanetExplorationHandler
    {
        private readonly RabbitHandler m_rabbitHandler;
        private readonly AppSettings m_appSettings;
        public PlanetExplorationHandler(RabbitHandler rabbitHandler, IOptions<AppSettings> appSettings)
        {
            m_appSettings = appSettings.Value;
            m_rabbitHandler = rabbitHandler;
        }

        public void HandleAsync(MessageType type)
        {
            m_rabbitHandler.ListenQueueAsync(new ListenOptions
            {
                MessageType = type,
                RequestParser = ExploreAsync,
                TargetQueue = m_appSettings.RabbitMqQueues.ExplorationQueue
            });
        }
        
        private async Task ExploreAsync(string body)
        {
            var request = JsonConvert.DeserializeObject<ExplorationRequest>(body);
            var response = new ExplorationResponse();

            m_rabbitHandler.PublishRpc(new PublishOptions
            {
                TargetQueue = m_appSettings.RabbitMqQueues.CrewApiQueue,
                Headers = new Dictionary<string, object>
                {
                    {nameof(MessageType), MessageType.UpdateRobotStatus},
                },
                Message = JsonConvert.SerializeObject(request.Robots)
            });
            
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

    }
}