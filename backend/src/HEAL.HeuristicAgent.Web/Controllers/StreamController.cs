using System.Globalization;
using System.Text.Json;
using System.Threading.Channels;
using HEAL.HeuristicAgent.Web.Dtos;
using HEAL.HeuristicAgent.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HEAL.HeuristicAgent.Web.Controllers;

[ApiController]
[Route("")]
public class StreamController(LlmResponseStream llmStream, IDataClient dataClient) : ControllerBase
{
    [HttpGet("ai-stream")]
    public async Task AiStream()
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");

        var channel = Channel.CreateUnbounded<string>();

        llmStream.OnMessage += Handler;

        try
        {
            await foreach (var msg in channel.Reader.ReadAllAsync(HttpContext.RequestAborted))
            {
                await Response.WriteAsync(msg, HttpContext.RequestAborted);
                await Response.Body.FlushAsync(HttpContext.RequestAborted);
            }
        }
        finally
        {
            llmStream.OnMessage -= Handler;
        }

        return;

        void Handler(EventType type, string? msg)
        {
            var dto = msg is not null
                ? new EventDto
                {
                    Message = msg,
                }
                : null;

            channel.Writer.TryWrite(
                $"""
                 event: {type.ToString().ToLowerInvariant()}
                 data: {(dto is null ? "" : JsonSerializer.Serialize(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))}


                 """
            );
        }
    }

    [HttpGet("quality-stream")]
    public async Task QualityStream()
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");

        var channel = Channel.CreateUnbounded<double>();

        HttpContext.RequestAborted.Register(() => channel.Writer.TryComplete());

        dataClient.DataReceived += Handler;

        try
        {
            await foreach (var quality in channel.Reader.ReadAllAsync(HttpContext.RequestAborted))
            {
                await Response.WriteAsync($"data: {quality.ToString(CultureInfo.InvariantCulture)}\n\n",
                    HttpContext.RequestAborted);
                await Response.Body.FlushAsync(HttpContext.RequestAborted);
            }
        }
        finally
        {
            dataClient.DataReceived -= Handler;
        }

        return;

        void Handler(object? sender, DataReceivedEventArgs e)
        {
            if (e.ModelQuality is not null)
            {
                channel.Writer.TryWrite(e.ModelQuality.Value);
            }
        }
    }
}
