namespace EnterpriseDataAnalyst.Application.DTOs;

public class DocumentChunk
{
    public string SourceId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
}
