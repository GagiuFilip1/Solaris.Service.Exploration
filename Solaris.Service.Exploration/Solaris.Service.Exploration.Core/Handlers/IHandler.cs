using System.Threading.Tasks;
using Solaris.Service.Exploration.Core.Enums;

namespace Solaris.Service.Exploration.Core.Handlers
{
    public interface IHandler
    { 
        void HandleAsync(MessageType type);
    }
}