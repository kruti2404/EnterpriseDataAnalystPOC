using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;
using EnterpriseDataAnalyst.Application.Interfaces;

namespace EnterpriseDataAnalyst.Infrastructure.Services;

public class PlannerAgent : IPlannerAgent
{
    private readonly IAiService _aiService;

    public PlannerAgent(IAiService aiService)
    {
        _aiService = aiService;
    }

    public async Task<Plan> PlanAsync(string question)
    {
        var prompt = $@"
You are a planning agent for an enterprise sales data analyst system.

DATABASE SCHEMA:
- Sales table: Id, Date (DateTime), Region (string: North/South/East/West), ProductId, CustomerId, Quantity (int), Amount (decimal)
- Product table: Id, Name (string), Category (string), UnitPrice, StockQty
- Customer table: Id, Name, Email, Region, JoinDate

DATA AVAILABLE (always fetched for every query):
- Sales grouped by ProductName + Category + Year: useful for product category trends, YoY growth, best/worst performers by product
- Sales grouped by Region + Year: useful for regional comparisons, regional trends over time

AVAILABLE ACTIONS:
- fetch-sales-data: Retrieves the sales breakdown (product+year and region+year). Always needed.
- fetch-docs: Retrieves internal document context (market reports, competitor analysis, operations notes). Use when the question asks about reasons, causes, or external factors.
- analyze: Synthesizes the fetched data and documents to answer the question with insights and charts.

USER QUESTION: '{question}'

Think step by step about what actions are needed to answer this question, then return ONLY valid JSON (no markdown):
{{
  ""Steps"": [
    {{ ""Action"": ""fetch-sales-data"", ""Description"": ""Retrieve product+year and region+year sales breakdowns"" }},
    {{ ""Action"": ""fetch-docs"", ""Description"": ""Retrieve market reports for context on causes"" }},
    {{ ""Action"": ""analyze"", ""Description"": ""Compute YoY growth by product category and identify the fastest growing"" }}
  ]
}}
Always include fetch-sales-data and analyze. Include fetch-docs only if contextual explanation is needed.
";
        return await _aiService.GenerateJsonAsync<Plan>(prompt);
    }
}
