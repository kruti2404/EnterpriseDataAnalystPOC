using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;

namespace EnterpriseDataAnalyst.Application.Interfaces;

public interface IValidationAgent
{
    Task<ValidationResult> ValidateAsync(InsightsResponse insights, List<SalesSummary> salesData);
}
