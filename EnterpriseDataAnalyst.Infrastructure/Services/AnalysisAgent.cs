using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;
using EnterpriseDataAnalyst.Application.Interfaces;

namespace EnterpriseDataAnalyst.Infrastructure.Services;

public class AnalysisAgent : IAnalysisAgent
{
    private readonly IAiService _aiService;

    public AnalysisAgent(IAiService aiService)
    {
        _aiService = aiService;
    }

    public async Task<InsightsResponse> AnalyzeAsync(
        string question,
        List<SalesSummary>? salesData,
        IReadOnlyList<DocumentChunk>? documentChunks,
        List<WebSearchResult>? webResults,
        List<ConversationMessage> history)
    {
        var hasDbData = salesData is { Count: > 0 };
        var hasWebData = webResults is { Count: > 0 };
        var hasDocData = documentChunks is { Count: > 0 };

        var prompt = BuildPrompt(question, salesData, documentChunks, webResults, history, hasDbData, hasWebData, hasDocData);
        return await _aiService.GenerateJsonAsync<InsightsResponse>(prompt);
    }

    private static string BuildPrompt(
        string question,
        List<SalesSummary>? salesData,
        IReadOnlyList<DocumentChunk>? documentChunks,
        List<WebSearchResult>? webResults,
        List<ConversationMessage> history,
        bool hasDbData,
        bool hasWebData,
        bool hasDocData)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an expert enterprise data analyst.");
        sb.AppendLine();

        // Conversation history
        if (history is { Count: > 0 })
        {
            sb.AppendLine("CONVERSATION HISTORY (prior context — maintain continuity in your answer):");
            foreach (var msg in history)
                sb.AppendLine($"{(msg.Role == "user" ? "User" : "Assistant")}: {msg.Content}");
            sb.AppendLine();
        }

        sb.AppendLine($"CURRENT USER QUESTION: '{question}'");
        sb.AppendLine();

        // Data sections — only include what was actually fetched
        if (hasDbData)
        {
            sb.AppendLine("COMPANY SALES DATA (from internal database):");
            sb.AppendLine("Each row is either:");
            sb.AppendLine("  (a) Product breakdown: ProductName, Category, Year, TotalSales, TotalQuantity");
            sb.AppendLine("  (b) Region breakdown: Region, Year, TotalSales, TotalQuantity");
            sb.AppendLine("Use actual numbers from this data in your answer. Compute YoY growth, rankings, totals as needed.");
            sb.AppendLine(JsonSerializer.Serialize(salesData));
            sb.AppendLine();
        }

        if (hasDocData)
        {
            sb.AppendLine("INTERNAL DOCUMENT CONTEXT:");
            sb.AppendLine(JsonSerializer.Serialize(documentChunks));
            sb.AppendLine();
        }

        if (hasWebData)
        {
            sb.AppendLine("WEB SEARCH RESULTS (external knowledge):");
            sb.AppendLine(JsonSerializer.Serialize(webResults));
            sb.AppendLine();
        }

        if (!hasDbData && !hasWebData && !hasDocData)
        {
            sb.AppendLine("No external data was fetched. Answer from your own knowledge.");
            sb.AppendLine();
        }

        // Chart instructions — clearly scoped to when charts make sense
        sb.AppendLine("CHART RULES:");
        if (hasDbData)
        {
            sb.AppendLine("- You HAVE quantitative database data, so you SHOULD include at least one chart that directly visualizes the answer.");
            sb.AppendLine("- Use 'line' for year-over-year trends. Use 'bar' for category/region comparisons. Use 'pie' for share/distribution. Use 'table' for ranked multi-column lists.");
        }
        else
        {
            sb.AppendLine("- You do NOT have quantitative database data. Only include a chart if the web search results contain specific numeric figures worth visualizing.");
            sb.AppendLine("- If the answer is purely textual/explanatory, return Charts as an empty array: []");
        }
        sb.AppendLine();

        sb.AppendLine("Return ONLY raw JSON (no markdown fences) exactly matching this structure:");
        sb.AppendLine(@"{
  ""Insights"": ""Clear specific answer with numbers where available."",
  ""Charts"": [
    {
      ""ChartType"": ""bar|line|pie|table"",
      ""Title"": ""Descriptive title"",
      ""Series"": [
        {
          ""Name"": ""Series label"",
          ""Data"": [
            { ""Label"": ""X label"", ""Value"": 123.45 }
          ]
        }
      ]
    }
  ],
  ""Sources"": [""Database"", ""DuckDuckGo""],
  ""DataSource"": ""Database | WebSearch | Mixed | Knowledge""
}");

        return sb.ToString();
    }
}
