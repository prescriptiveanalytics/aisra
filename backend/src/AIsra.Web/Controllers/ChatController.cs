using AIsra.Web.Dtos;
using AIsra.Web.Services.Chat;
using Microsoft.AspNetCore.Mvc;

namespace AIsra.Web.Controllers;

[ApiController]
[Route("chat")]
public sealed class ChatController(ILlmClient llmClient) : ControllerBase
{
    /// <summary>
    /// Sends a chat message to the AI agent.
    /// </summary>
    [HttpPost]
    public IActionResult Chat([FromBody] ChatRequest request)
    {
        if (llmClient.AgentIsBusy)
        {
            return BadRequest("Agent is currently busy. Please try again later.");
        }

        llmClient.ChatAsync(request.Message, HttpContext.RequestAborted).Forget();

        return Accepted();
    }
}
