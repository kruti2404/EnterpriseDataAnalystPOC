using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EnterpriseDataAnalyst.Application.DTOs;
using EnterpriseDataAnalyst.Application.Interfaces;

namespace EnterpriseDataAnalyst.Infrastructure.Services;

public class OrchestratorService : IOrchestratorService
{
    private readonly IRouterAgent _routerAgent;
    private readonly IPlannerAgent _plannerAgent;
    private readonly IRagAgent _ragAgent;
    private readonly IDataAgent _dataAgent;
    private readonly IAnalysisAgent _analysisAgent;
    private readonly IValidationAgent _validationAgent;
    private readonly IWebSearchAgent _webSearchAgent;
    private readonly ILogger<OrchestratorService> _logger;

    public OrchestratorService(
        IRouterAgent routerAgent,
        IPlannerAgent plannerAgent,
        IRagAgent ragAgent,
        IDataAgent dataAgent,
        IAnalysisAgent analysisAgent,
        IValidationAgent validationAgent,
        IWebSearchAgent webSearchAgent,
        ILogger<OrchestratorService> logger)
    {
        _routerAgent = routerAgent;
        _plannerAgent = plannerAgent;
        _ragAgent = ragAgent;
        _dataAgent = dataAgent;
        _analysisAgent = analysisAgent;
        _validationAgent = validationAgent;
        _webSearchAgent = webSearchAgent;
        _logger = logger;
    }

    public async Task<InsightsResponse> AnalyzeQuestionAsync(string question, List<ConversationMessage> history)
    {
        _logger.LogInformation("Received question: {Question}", question);

        // Step 1: Route — decide where to get data from
        RouteDecision route;
        try
        {
            route = await _routerAgent.RouteAsync(question);
            _logger.LogInformation("Route decision: DB={NeedsDb}, Web={NeedsWeb}, Reason={Reason}",
                route.NeedsDatabase, route.NeedsWebSearch, route.Reasoning);
        }
        catch (Exception ex)
        {
            // If the router itself fails (e.g. LLM parse error), default to Knowledge-only so the user still gets an answer
            _logger.LogWarning(ex, "RouterAgent failed — defaulting to Knowledge-only route");
            route = new RouteDecision { NeedsDatabase = false, NeedsWebSearch = false, Reasoning = "fallback" };
        }

        List<SalesSummary> salesData = new();
        IReadOnlyList<DocumentChunk> documentChunks = new List<DocumentChunk>();
        List<WebSearchResult> webResults = new();

        // Step 2a: Fetch from DB (parallel plan + data + docs)
        if (route.NeedsDatabase)
        {
            var plan = await _plannerAgent.PlanAsync(question);
            _logger.LogInformation("Generated plan with {Count} steps", plan?.Steps?.Count ?? 0);

            var ragTask = _ragAgent.RetrieveContextAsync(question);
            var dataTask = _dataAgent.FetchDataAsync(plan!);
            await Task.WhenAll(ragTask, dataTask);

            documentChunks = await ragTask;
            salesData = await dataTask;
            _logger.LogInformation("DB fetch: {DocCount} docs, {DataCount} sales rows", documentChunks.Count, salesData.Count);
        }

        // Step 2b: Fetch from web (can run in parallel with DB fetch if both needed)
        if (route.NeedsWebSearch)
        {
            var searchQuery = !string.IsNullOrWhiteSpace(route.SearchQuery) ? route.SearchQuery : question;
            try
            {
                webResults = await _webSearchAgent.SearchAsync(searchQuery);
                _logger.LogInformation("Web search returned {Count} results for query: {Query}", webResults.Count, searchQuery);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Web search failed for query '{Query}' — continuing without web results", searchQuery);
            }
        }

        // Step 3: Analyze with all available context
        var insights = await _analysisAgent.AnalyzeAsync(question, salesData, documentChunks, webResults, history);
        _logger.LogInformation("Generated insights, charts: {ChartCount}", insights?.Charts?.Count ?? 0);

        // Step 4: Populate structured source links from real data (not LLM-generated, so URLs are guaranteed real)
        insights!.SourceLinks = BuildSourceLinks(salesData, documentChunks, webResults);

        // Step 5: Validate only when we have DB data to fact-check against
        if (route.NeedsDatabase && salesData.Count > 0)
        {
            var validation = await _validationAgent.ValidateAsync(insights, salesData);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Validation flagged: {Issues}", string.Join(", ", validation.Issues));
                insights.Insights += "\n\nNote: " + string.Join("; ", validation.Issues);
            }
        }

        return insights;
    }

    private static List<SourceLink> BuildSourceLinks(
        List<SalesSummary> salesData,
        IReadOnlyList<DocumentChunk> documentChunks,
        List<WebSearchResult> webResults)
    {
        var links = new List<SourceLink>();

        // Real web URLs from search results — always trustworthy
        foreach (var r in webResults.Where(r => !string.IsNullOrWhiteSpace(r.Url)))
        {
            links.Add(new SourceLink
            {
                Title = r.Title,
                Url = r.Url,
                Snippet = r.Snippet,
                Type = "web"
            });
        }

        // Internal database reference
        if (salesData.Count > 0)
        {
            links.Add(new SourceLink
            {
                Title = "Internal Sales Database",
                Url = string.Empty,
                Snippet = $"{salesData.Count} aggregated records across products and regions.",
                Type = "database"
            });
        }

        // Internal documents
        foreach (var doc in documentChunks.Where(d => !string.IsNullOrWhiteSpace(d.SourceId)))
        {
            links.Add(new SourceLink
            {
                Title = doc.SourceId,
                Url = string.Empty,
                Snippet = doc.Content.Length > 120 ? doc.Content[..120] + "…" : doc.Content,
                Type = "document"
            });
        }

        return links;
    }
}
