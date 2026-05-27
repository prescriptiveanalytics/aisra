using System.Text.Json;
using System.Threading.Channels;
using HEAL.HeuristicAgent.Web.Dtos;
using HEAL.HeuristicAgent.Web.Persistence;
using HEAL.HeuristicAgent.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HEAL.HeuristicAgent.Web.Controllers;

[ApiController]
[Route("")]
public sealed class StreamController(
    LlmResponseStream llmStream,
    IDataClient dataClient,
    IDataStore dataStore,
    IModelService modelService,
    IModelAnalysisService modelAnalysisService) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

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
                 data: {(dto is null ? "" : JsonSerializer.Serialize(dto, JsonOptions))}


                 """
            );
        }
    }

    [HttpGet("metrics-stream")]
    public async Task MetricsStream([FromQuery] int? modelId = null, CancellationToken ct = default)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");

        var channel = Channel.CreateUnbounded<ModelMetricsDto>();
        var eventChannel = Channel.CreateUnbounded<double[]>();

        HttpContext.RequestAborted.Register(() => 
        {
            channel.Writer.TryComplete();
            eventChannel.Writer.TryComplete();
        });

        dataClient.DataReceived += Handler;

        Task.Run(async () =>
        {
            try
            {
                await foreach (var _ in eventChannel.Reader.ReadAllAsync(HttpContext.RequestAborted))
                {
                    var recentData = await dataStore
                        .GetLastAsync(20)
                        .Select(x => x.Item2)
                        .ToArrayAsync(cancellationToken: ct);

                    if (recentData.Length < 20)
                    {
                        continue;
                    }

                    var combinedModel = await modelService.GetCombinedModelAsync(modelId, ct);

                    channel.Writer.TryWrite(
                        new ModelMetricsDto(
                            modelAnalysisService.EvaluateQuality(combinedModel, recentData),
                            modelAnalysisService
                                .CalculatePermutationFeatureImportance(combinedModel, recentData)
                                .Select((d, i) => new FeatureImportanceDto(
                                    i == recentData[0].Length - 1 ? "y" : $"x{i + 1}",
                                    d
                                ))
                                .ToArray()
                        )
                    );
                }
            }
            catch (OperationCanceledException)
            {
            }
        }, ct).Forget();

        try
        {
            await foreach (var quality in channel.Reader.ReadAllAsync(HttpContext.RequestAborted))
            {
                await Response.WriteAsync($"data: {
                    JsonSerializer.Serialize(quality, JsonOptions)
                }\n\n", HttpContext.RequestAborted);
                await Response.Body.FlushAsync(HttpContext.RequestAborted);
            }
        }
        finally
        {
            dataClient.DataReceived -= Handler;
        }

        return;

        void Handler(object? sender, double[] e)
        {
            eventChannel.Writer.TryWrite(e);
        }
    }
}
