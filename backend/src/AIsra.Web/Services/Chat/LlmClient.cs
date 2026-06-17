using AIsra.Web.Dtos;
using AIsra.Web.Services.Data;
using AIsra.Web.Services.Modeling;
using AIsra.Web.Services.Persistence;
using AIsra.Common.Threading;
using Microsoft.Extensions.AI;

namespace AIsra.Web.Services.Chat;

public sealed class LlmClient(
    IDataClient dataClient,
    IDataStorage dataStorage,
    IHeuristicChatClient chatClient,
    ApplicationEventStream responseStream,
    IModelService modelService,
    IModelAnalyzer modelAnalyzer,
    ICancellationTokenProvider ctp
) : ILlmClient
{
    private const int NumValuesToUse = 20;

    private static readonly double QualityThreshold = 90.Percent;

    private readonly SemaphoreSlim agentGate = new(1, 1);

    public bool AgentIsBusy => agentGate.CurrentCount == 0;

    public Task StartAsync(CancellationToken ct)
    {
        dataClient.DataReceived += async (_, _) =>
        {
            var data = await dataStorage
                .GetLastDataAsync(NumValuesToUse)
                .Select(c => c.Item2)
                .ToArrayAsync(ct);

            if (data.Length < NumValuesToUse)
            {
                return;
            }

            var combinedModel = await modelService.GetCombinedModelAsync(null, ctp.Token);

            if (combinedModel is null)
            {
                return;
            }

            var quality = modelAnalyzer.EvaluateQuality(combinedModel, data);

            if (quality > QualityThreshold)
            {
                return;
            }

            if (!await agentGate.WaitAsync(0, ct))
            {
                return;
            }

            try
            {
                responseStream.Broadcast(EventType.QualityDrop);

                await foreach (
                    var s in chatClient.GetStreamingResponseAsync([
                        new ChatMessage(
                            ChatRole.User,
                            $"[{DateTimeOffset.UtcNow}] [SYSTEM]" +
                            $" The model quality fell below the threshold of {QualityThreshold} (quality: {quality:F2})."
                        ),
                    ]).WithCancellation(ct)
                )
                {
                    responseStream.Broadcast(EventType.Fragment, s);
                }

                responseStream.Broadcast(EventType.Done);
            }
            finally
            {
                agentGate.Release();
            }
        };

        return Task.CompletedTask;
    }

    public async Task ChatAsync(string message, CancellationToken ct)
    {
        if (!await agentGate.WaitAsync(0, ct))
        {
            return;
        }

        try
        {
            await foreach (
                var s in chatClient.GetStreamingResponseAsync([
                    new ChatMessage(ChatRole.User, message),
                ]).WithCancellation(ct)
            )
            {
                responseStream.Broadcast(EventType.Fragment, s);
            }

            responseStream.Broadcast(EventType.Done);
        }
        finally
        {
            agentGate.Release();
        }
    }

    public Task StopAsync(CancellationToken ct)
        => Task.CompletedTask;
}
