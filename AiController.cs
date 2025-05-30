using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SDMS.Core.DTOs;
using System;
using System.Threading.Tasks;

namespace SDMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiController : ControllerBase
    {
        private readonly ILogger<AiController> _logger;

        // Inject other services like an AI service if needed
        public AiController(ILogger<AiController> logger)
        {
            _logger = logger;
        }

        [HttpPost("ask")]
        [AllowAnonymous] // Public endpoint as requested
        public async Task<IActionResult> AskAi([FromBody] AiQuestionDto questionDto)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(questionDto.Question))
            {
                return BadRequest(new { message = "Invalid question provided." });
            }

            _logger.LogInformation("Received AI question: {Question}", questionDto.Question);

            try
            {
                // --- Placeholder for AI interaction --- 
                // Replace this with actual call to an AI service (e.g., OpenAI)
                // For now, returning a simulated response.
                await Task.Delay(150); // Simulate AI processing time

                string simulatedAnswer = $"This is a simulated answer to your question about: ";
                if (questionDto.Question.Contains("customer churn", StringComparison.OrdinalIgnoreCase))
                {
                    simulatedAnswer += "reducing customer churn. Consider implementing loyalty programs, improving customer support, and gathering feedback.";
                }
                else if (questionDto.Question.Contains("funding", StringComparison.OrdinalIgnoreCase))
                {
                    simulatedAnswer += "startup funding. Explore options like venture capital, angel investors, and crowdfunding.";
                }
                else
                {
                    simulatedAnswer += $"'{questionDto.Question}'. More detailed analysis would require a real AI model.";
                }
                
                // --- End Placeholder --- 

                return Ok(new { answer = simulatedAnswer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the AI question: {Question}", questionDto.Question);
                return StatusCode(500, new { message = "An internal error occurred while processing your question." });
            }
        }
    }
}

