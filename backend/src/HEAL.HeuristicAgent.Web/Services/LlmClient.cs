using HEAL.HeuristicAgent.Web.Chat;
using HEAL.HeuristicAgent.Web.Dtos;
using HEAL.HeuristicAgent.Web.Persistence;
using Microsoft.Extensions.AI;

namespace HEAL.HeuristicAgent.Web.Services;

public sealed class LlmClient(
    IDataClient dataClient,
    IDataStore dataStore,
    IHeuristicChatClient chatClient,
    LlmResponseStream responseStream,
    IModelService modelService,
    IModelAnalysisService modelAnalysisService
) : IHostedService
{
    private const int NumValuesToUse = 20;

    private static readonly double QualityThreshold = 90.Percent;

    private readonly SemaphoreSlim _agentGate = new(1, 1);

    public bool AgentIsBusy => _agentGate.CurrentCount == 0;

    public Task StartAsync(CancellationToken ct)
    {
        dataClient.DataReceived += async (_, _) =>
        {
            var data = await dataStore
                .GetLastAsync(NumValuesToUse)
                .Select(c => c.Item2)
                .ToArrayAsync(ct);

            if (data.Length < NumValuesToUse)
            {
                return;
            }

            var combinedModel = await modelService.GetCombinedModelAsync();
            var quality = modelAnalysisService.EvaluateQuality(combinedModel, data);

            if (quality > QualityThreshold)
            {
                return;
            }

            if (!await _agentGate.WaitAsync(0, ct))
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
                        )
                    ]).WithCancellation(ct)
                )
                {
                    responseStream.Broadcast(EventType.Fragment, s);
                }

                responseStream.Broadcast(EventType.Done);
            }
            finally
            {
                _agentGate.Release();
            }
        };

        return Task.CompletedTask;
    }

    public async Task ChatAsync(string message, CancellationToken ct = default)
    {
        if (!await _agentGate.WaitAsync(0, ct))
        {
            return;
        }

        try
        {
            await foreach (
                var s in chatClient.GetStreamingResponseAsync([
                    new ChatMessage(ChatRole.User, message)
                ]).WithCancellation(ct)
            )
            {
                responseStream.Broadcast(EventType.Fragment, s);
            }

            responseStream.Broadcast(EventType.Done);
        }
        finally
        {
            _agentGate.Release();
        }
    }

    public Task StopAsync(CancellationToken ct)
        => Task.CompletedTask;
}
