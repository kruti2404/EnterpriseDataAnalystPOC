namespace EnterpriseDataAnalyst.Application.DTOs;

public class ConversationMessage
{
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
}
