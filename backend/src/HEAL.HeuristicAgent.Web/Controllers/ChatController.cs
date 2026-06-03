using HEAL.HeuristicAgent.Web.Dtos;
using HEAL.HeuristicAgent.Web.Services;
using HEAL.HeuristicAgent.Web.Services.Chat;
using Microsoft.AspNetCore.Mvc;

namespace HEAL.HeuristicAgent.Web.Controllers;

[ApiController]
[Route("")]
public class ChatController(LlmClient llmClient) : ControllerBase
{
    [HttpPost("chat")]
    public IActionResult Chat([FromBody] ChatRequest request)
    {
        if (llmClient.AgentIsBusy)
        {
            return BadRequest("Agent is currently busy. Please try again later.");
        }

        llmClient.ChatAsync(request.Message).Forget();

        return Accepted();
    }
}
