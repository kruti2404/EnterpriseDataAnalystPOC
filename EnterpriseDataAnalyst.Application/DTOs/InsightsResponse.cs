using System.Collections.Generic;

namespace EnterpriseDataAnalyst.Application.DTOs;

public class InsightsResponse
{
    public string Insights { get; set; } = string.Empty;
    public List<ChartDto> Charts { get; set; } = new();
    public List<string> Sources { get; set; } = new();
    /// <summary>"Database" | "WebSearch" | "Mixed" | "Knowledge"</summary>
    public string DataSource { get; set; } = string.Empty;
    /// <summary>Structured, clickable source references populated from actual search/DB results.</summary>
    public List<SourceLink> SourceLinks { get; set; } = new();
}

public class SourceLink
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    /// <summary>"web" | "database" | "document"</summary>
    public string Type { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
}

public class ChartDto
{
    public string ChartType { get; set; } = string.Empty; // bar|line|pie|table
    public string Title { get; set; } = string.Empty;
    public List<ChartSeries> Series { get; set; } = new();
}

public class ChartSeries
{
    public string Name { get; set; } = string.Empty;
    public List<ChartDataPoint> Data { get; set; } = new();
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
