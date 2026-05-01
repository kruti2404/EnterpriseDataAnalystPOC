using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;

namespace EnterpriseDataAnalyst.Application.Interfaces;

public interface IRouterAgent
{
    Task<RouteDecision> RouteAsync(string question);
}
