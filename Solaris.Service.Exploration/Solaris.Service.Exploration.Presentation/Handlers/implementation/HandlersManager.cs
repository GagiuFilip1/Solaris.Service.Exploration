using System.Collections.Generic;
using Solaris.Service.Exploration.Core.Handlers.Interfaces;
using Solaris.Service.Exploration.Infrastructure.Ioc;

namespace Solaris.Service.Exploration.Presentation.Handlers.implementation
{
    [RegistrationKind(Type = RegistrationType.Scoped, AsSelf = true)]
    public class HandlersManager
    {
        private readonly IEnumerable<IHandler> m_handlers;

        public HandlersManager(IEnumerable<IHandler> handlers)
        {
            m_handlers = handlers;
        }

        public void HandleRequests()
        {
            foreach (var handler in m_handlers)
            {
                handler.HandleAsync();
            }
        }
    }
}