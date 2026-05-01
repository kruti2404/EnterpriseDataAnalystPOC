using System.Collections.Generic;

namespace EnterpriseDataAnalyst.Application.DTOs;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Issues { get; set; } = new();
    public InsightsResponse? CorrectedInsights { get; set; }
}
