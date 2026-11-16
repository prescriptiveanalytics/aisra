using AIsra.Web.Dtos;
using AIsra.Web.Services;
using AIsra.Web.Services.Chat;
using Microsoft.AspNetCore.Mvc;

namespace AIsra.Web.Controllers;

[ApiController]
[Route("chat")]
public sealed class ChatController(ILlmClient llmClient, Settings settings) : ControllerBase
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

        llmClient.ChatAsync(request.Message).Forget();

        return Accepted();
    }

    /// <summary>
    /// Interrupts the AI agent if it is currently responding.
    /// </summary>
    /// <returns><see langword="true"/> if a running response was interrupted.</returns>
    [HttpPost("stop")]
    public bool Stop()
        => llmClient.Interrupt();

    /// <summary>
    /// Gets the agent settings that can be changed by the user.
    /// </summary>
    [HttpGet("settings")]
    public AgentSettingsDto GetSettings()
        => new()
        {
            MonitorModelQuality = settings.MonitorModelQuality,
        };

    /// <summary>
    /// Updates the agent settings that can be changed by the user.
    /// </summary>
    [HttpPut("settings")]
    public AgentSettingsDto UpdateSettings([FromBody] AgentSettingsDto request)
    {
        settings.MonitorModelQuality = request.MonitorModelQuality;

        return GetSettings();
    }
}
