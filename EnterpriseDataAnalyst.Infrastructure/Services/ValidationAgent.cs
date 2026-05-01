using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;
using EnterpriseDataAnalyst.Application.Interfaces;

namespace EnterpriseDataAnalyst.Infrastructure.Services;

public class ValidationAgent : IValidationAgent
{
    private readonly IAiService _aiService;

    public ValidationAgent(IAiService aiService)
    {
        _aiService = aiService;
    }

    public async Task<ValidationResult> ValidateAsync(InsightsResponse insights, List<SalesSummary> salesData)
    {
        var insightsJson = JsonSerializer.Serialize(insights);
        var salesJson = JsonSerializer.Serialize(salesData);

        var prompt = $@"
You are a validation agent. Check if the insights and charts generated match the actual data.
Insights Generated:
{insightsJson}

Actual Data:
{salesJson}

Return a JSON object in this format ONLY:
{{
  ""IsValid"": true,
  ""Issues"": [
    ""List any hallucinations or mismatches here, or leave empty if valid.""
  ]
}}
Do not include markdown fences.
";
        return await _aiService.GenerateJsonAsync<ValidationResult>(prompt);
    }
}
