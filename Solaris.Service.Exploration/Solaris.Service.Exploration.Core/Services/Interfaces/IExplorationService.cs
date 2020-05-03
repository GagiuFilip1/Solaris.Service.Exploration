using System.Threading.Tasks;
using Solaris.Service.Exploration.Core.Models.Requests;

namespace Solaris.Service.Exploration.Core.Services.Interfaces
{
    public interface IExplorationService
    {
        Task ExplorePlanet(ExplorationResponse response);
    }
}