using System.Collections.Generic;
using System.Threading.Tasks;
using Solaris.Service.Exploration.Core.Models.Entities;
using Solaris.Service.Exploration.Core.Models.Requests;

namespace Solaris.Service.Exploration.Core.Services
{
    public interface IExplorationService
    {
        Task ExplorePlanet(ExplorationResponse response);
    }
}