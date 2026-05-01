using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;
using EnterpriseDataAnalyst.Application.Interfaces;

namespace EnterpriseDataAnalyst.Infrastructure.Services;

public class RouterAgent : IRouterAgent
{
    private readonly IAiService _aiService;

    public RouterAgent(IAiService aiService)
    {
        _aiService = aiService;
    }

    public async Task<RouteDecision> RouteAsync(string question)
    {
        var prompt = $@"
You are a routing agent for an enterprise sales data analyst system.

Your job is to decide where to get data to answer the user's question.

INTERNAL DATABASE contains:
- Company-specific sales records: product sales by region, by year, revenue, quantities, product categories
- Customer records: names, regions, join dates
- Product catalog: names, categories, prices, stock levels
- This data is PRIVATE to the company — not available anywhere else

WEB SEARCH can retrieve:
- Industry trends, market benchmarks, competitor information
- General business knowledge, economic conditions
- News, reports, public statistics not in the company database
- Explanations of external factors (supply chains, seasonality, regulations)

RULES:
- NeedsDatabase = true  → the question asks about THIS COMPANY'S OWN sales, products, customers, revenue, or regional performance
- NeedsWebSearch = true → the question asks about industry context, external trends, general knowledge, or WHY something happened externally
- Both can be true → e.g. 'How do our sales compare to industry benchmarks?' or 'Why did West region sales drop?' (needs our data + external causes)
- NeedsDatabase = false, NeedsWebSearch = false → the question is a simple factual/knowledge question the AI can answer from its own training (e.g. 'What is EBITDA?')
- SearchQuery → a concise, optimised search query string for web search (only relevant when NeedsWebSearch=true)

USER QUESTION: '{question}'

Return ONLY raw JSON (no markdown):
{{
  ""NeedsDatabase"": true,
  ""NeedsWebSearch"": false,
  ""SearchQuery"": """",
  ""Reasoning"": ""One sentence explaining why""
}}
";
        return await _aiService.GenerateJsonAsync<RouteDecision>(prompt);
    }
}
