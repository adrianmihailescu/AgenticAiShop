using AiShoppingAgent.Api.Models;
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
        Console.WriteLine("AgentController initialized");
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] AgentRequest request)
    {
        Console.WriteLine("Run() hit");
        Console.WriteLine("Prompt = " + request?.Prompt);

        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest("Prompt is required.");

        Console.WriteLine("Calling GeminiService.HandleUserQueryAsync");
        Console.WriteLine("Prompt = " + request.Prompt);
        Console.WriteLine(request);

        try
        {
            var result = await _geminiService.HandleUserQueryAsync(request.Prompt);
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR in Run():");
            Console.WriteLine(ex.ToString());
            return StatusCode(500, ex.ToString());
        }
    }
}
