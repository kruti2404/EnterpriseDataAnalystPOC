using System.Collections.Generic;

namespace EnterpriseDataAnalyst.Application.DTOs;

public class AnalyzeRequest
{
    public string Question { get; set; } = string.Empty;
    public List<ConversationMessage> History { get; set; } = new();
}
