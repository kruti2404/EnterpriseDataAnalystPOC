using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using EnterpriseDataAnalyst.Application.DTOs;
using EnterpriseDataAnalyst.Application.Interfaces;

namespace EnterpriseDataAnalyst.API.Controllers;

[ApiController]
[Route("api/ai")]
public class AIController : ControllerBase
{
    private readonly IOrchestratorService _orchestrator;
    private readonly ILogger<AIController> _logger;

    public AIController(IOrchestratorService orchestrator, ILogger<AIController> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<InsightsResponse>> Analyze([FromBody] AnalyzeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "Question cannot be empty." });

        try
        {
            var result = await _orchestrator.AnalyzeQuestionAsync(request.Question, request.History ?? new());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Orchestrator failed for question: {Question}", request.Question);
            return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
        }
    }
}
