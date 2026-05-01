using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;

namespace EnterpriseDataAnalyst.Application.Interfaces;

public interface IOrchestratorService
{
    Task<InsightsResponse> AnalyzeQuestionAsync(string question, List<ConversationMessage> history);
}
