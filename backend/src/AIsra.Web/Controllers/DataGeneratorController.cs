using AIsra.Web.Services.DataGeneration;
using DsgWeb.Client;
using Microsoft.AspNetCore.Mvc;

namespace AIsra.Web.Controllers;

/// <summary>
/// Forwards requests to the data stream generator so the frontend can configure, start, and stop the data stream.
/// </summary>
[ApiController]
[Route("generator")]
public sealed class DataGeneratorController(IDsgWebClient client, DataGeneratorService service) : ControllerBase
{
    [HttpGet("status")]
    public Task<IActionResult> GetStatus(CancellationToken ct)
        => ForwardAsync(() => client.GetStatusAsync(ct));

    /// <summary>
    /// Gets the configuration stored in the generator, or the default configuration if none has been stored yet.
    /// </summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfiguration(CancellationToken ct)
    {
        try
        {
            return Ok(await client.GetConfigAsync(ct));
        }
        catch (ApiException e) when (e.StatusCode == StatusCodes.Status404NotFound)
        {
            return Ok(service.GetDefaultConfiguration());
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            return ToErrorResult(e);
        }
    }

    [HttpGet("config/default")]
    public IActionResult GetDefaultConfiguration()
        => Ok(service.GetDefaultConfiguration());

    [HttpPost("config")]
    public Task<IActionResult> SetConfiguration([FromBody] DsgConfigurationRequest configuration, CancellationToken ct)
        => ForwardAsync(() => client.PostConfigAsync(configuration, ct));

    [HttpPost("start")]
    public Task<IActionResult> Start(CancellationToken ct)
        => ForwardAsync(() => client.StartStreamAsync(ct));

    [HttpPost("stop")]
    public Task<IActionResult> Stop(CancellationToken ct)
        => ForwardAsync(() => client.StopStreamAsync(ct));

    [HttpPost("restart")]
    public async Task<IActionResult> Restart(CancellationToken ct)
    {
        try
        {
            return Ok(await service.RestartAsync(ct));
        }
        catch (TimeoutException e)
        {
            return Error(StatusCodes.Status504GatewayTimeout, e.Message);
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            return ToErrorResult(e);
        }
    }

    private async Task<IActionResult> ForwardAsync<T>(Func<Task<T>> request)
    {
        try
        {
            return Ok(await request());
        }
        catch (Exception e) when (e is ApiException or HttpRequestException)
        {
            return ToErrorResult(e);
        }
    }

    /// <summary>
    /// Passes errors of the generator on to the frontend.
    /// </summary>
    private ObjectResult ToErrorResult(Exception exception) => exception switch
    {
        ApiException<ErrorResponse> { Result.Error: { } message } e => Error(e.StatusCode, message),
        ApiException e => Error(e.StatusCode, "The data stream generator returned an unexpected response."),
        _ => Error(StatusCodes.Status502BadGateway, "The data stream generator is unreachable."),
    };

    private ObjectResult Error(int statusCode, string message)
        => StatusCode(statusCode, new ErrorResponse { Error = message });
}
