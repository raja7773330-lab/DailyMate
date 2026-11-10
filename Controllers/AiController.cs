
using Microsoft.AspNetCore.Mvc;
using RoutinelyApi.Services;
using System.ComponentModel.DataAnnotations;

namespace RoutinelyApi.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly GeminiService _geminiService;

    public AiController(GeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _geminiService.GenerateResponseAsync(
                request.Message,
                cancellationToken);

            return Ok(new
            {
                success = true,
                message = response
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new
            {
                success = false,
                message = "Gemini API request failed.",
                error = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "An unexpected AI service error occurred."
            });
        }
    }
}

public class ChatRequest
{
    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
}
