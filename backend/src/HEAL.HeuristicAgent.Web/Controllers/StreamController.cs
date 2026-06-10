using System.Text.Json;
using System.Threading.Channels;
using HEAL.HeuristicAgent.Web.Dtos;
using HEAL.HeuristicAgent.Web.Services;
using HEAL.HeuristicAgent.Web.Services.Chat;
using HEAL.HeuristicAgent.Web.Services.Data;
using HEAL.HeuristicAgent.Web.Services.Modeling;
using HEAL.HeuristicAgent.Web.Services.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace HEAL.HeuristicAgent.Web.Controllers;

[ApiController]
[Route("")]
public sealed class StreamController(
    ApplicationEventStream llmStream,
    IDataClient dataClient,
    IDataAggregator dataAggregator,
    IDataStorage dataStorage,
    IModelService modelService,
    IModelAnalyzer modelAnalyzer,
    Settings features
) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// SSE endpoint for streaming LLM responses.
    /// </summary>
    [HttpGet("token-stream")]
    public async Task TokenStream()
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
            var data = msg is null ? "" : JsonSerializer.Serialize(new EventDto(msg), JsonOptions);

            channel.Writer.TryWrite(
                $"""
                 event: {type.ToString().ToLowerInvariant()}
                 data: {data}


                 """
            );
        }
    }

    /// <summary>
    /// SSE endpoint for streaming model metrics like quality and feature importance.
    /// </summary>
    /// <param name="modelId">The ID of the model to evaluate, or <see langword="null"/> to use the active model.</param>
    [HttpGet("metrics-stream")]
    public async Task MetricsStream([FromQuery] int? modelId = null)
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

        dataAggregator.DataAggregated += Handler;

        Task.Factory.StartNew(async () =>
        {
            try
            {
                await foreach (var _ in eventChannel.Reader.ReadAllAsync(HttpContext.RequestAborted))
                {
                    var recentData = await dataStorage
                        .GetLastAsync(50)
                        .Select(x => x.Item2)
                        .ToArrayAsync(cancellationToken: HttpContext.RequestAborted);

                    if (recentData.Length < 20)
                    {
                        continue;
                    }

                    var combinedModel = await modelService.GetCombinedModelAsync(modelId, HttpContext.RequestAborted);

                    if (combinedModel is null)
                    {
                        continue;
                    }

                    channel.Writer.TryWrite(
                        new ModelMetricsDto(
                            modelAnalyzer.EvaluateQuality(combinedModel, recentData.Take(20).ToArray()),
                            !features.FeatureImportance || recentData.Length < 50 ? null : modelAnalyzer
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
        }, HttpContext.RequestAborted, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap().Forget();

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
            dataAggregator.DataAggregated -= Handler;
        }

        return;

        void Handler(object? sender, double[] e)
        {
            eventChannel.Writer.TryWrite(e);
        }
    }

    [HttpGet("data-stream")]
    public async Task DataStream()
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");

        var channel = Channel.CreateUnbounded<DataPointDto>();

        dataClient.DataReceived += Handler;

        try
        {
            await foreach (var data in channel.Reader.ReadAllAsync(HttpContext.RequestAborted))
            {
                await Response.WriteAsync($"data: {JsonSerializer.Serialize(data, JsonOptions)}\n\n", HttpContext.RequestAborted);
                await Response.Body.FlushAsync(HttpContext.RequestAborted);
            }
        }
        finally
        {
            dataClient.DataReceived -= Handler;
        }

        return;

        void Handler(object? sender, DataPointDto e)
        {
            channel.Writer.TryWrite(e);
        }
    }
}
