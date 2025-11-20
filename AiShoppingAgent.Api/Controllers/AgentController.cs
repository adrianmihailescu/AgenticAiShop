using AiShoppingAgent.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiShoppingAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly GeminiService _geminiService;

    public AgentController(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    public class AgentRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] AgentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest("Prompt is required.");

        var result = await _geminiService.HandleUserQueryAsync(request.Prompt);
        return Ok(result);
    }
}
