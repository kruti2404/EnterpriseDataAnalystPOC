using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;

namespace EnterpriseDataAnalyst.Application.Interfaces;

public interface IPlannerAgent
{
    Task<Plan> PlanAsync(string question);
}
