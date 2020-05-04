using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Solaris.Service.Exploration.Core.Enums;
using Solaris.Service.Exploration.Core.Rabbit.Helpers.Responses;
using Solaris.Service.Exploration.Core.Rabbit.Helpers.Setup;
using Solaris.Service.Exploration.Core.Rabbit.Models;

namespace Solaris.Service.Exploration.Core.Rabbit.Interfaces
{
    public interface IRabbitHandler
    {
        public Dictionary<MessageType, Func<string, Task<RabbitResponse>>> Processors { get; }
        T PublishRpc<T>(PublishOptions options);
        void PublishRpc(PublishOptions options);
        void Publish(PublishOptions options);
        void ListenQueueAsync(ListenOptions options);
        void DeclareRpcQueue(QueueSetup setup);
        void DeclareQueue(QueueSetup queueSetup);
    }
}