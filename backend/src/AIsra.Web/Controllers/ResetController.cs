using AIsra.Web.Services.Reset;
using Microsoft.AspNetCore.Mvc;

namespace AIsra.Web.Controllers;

[ApiController]
[Route("reset")]
public sealed class ResetController(ApplicationResetService resetService) : ControllerBase
{
    /// <summary>
    /// Resets the entire application: stops the agent and the data stream, clears its message history, deletes all data and models,
    /// and restores the default settings.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Reset()
    {
        await resetService.ResetAsync();

        return NoContent();
    }
}
