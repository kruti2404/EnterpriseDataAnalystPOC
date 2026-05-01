namespace EnterpriseDataAnalyst.Application.DTOs;

public class RouteDecision
{
    public bool NeedsDatabase { get; set; }
    public bool NeedsWebSearch { get; set; }
    public string SearchQuery { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
}
