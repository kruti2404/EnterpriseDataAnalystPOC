using System.Collections.Generic;

namespace EnterpriseDataAnalyst.Application.DTOs;

public class Plan
{
    public List<Step> Steps { get; set; } = new();
}

public class Step
{
    public string Action { get; set; } = string.Empty; // fetch-sales-data, fetch-docs, analyze
    public string Description { get; set; } = string.Empty;
}
