using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;

namespace EnterpriseDataAnalyst.Application.Interfaces;

public interface IAnalysisAgent
{
    Task<InsightsResponse> AnalyzeAsync(
        string question,
        List<SalesSummary>? salesData,
        IReadOnlyList<DocumentChunk>? documentChunks,
        List<WebSearchResult>? webResults,
        List<ConversationMessage> history);
}
