using HEAL.HeuristicAgent.Web.Chat;
using HEAL.HeuristicAgent.Web.Dtos;
using Microsoft.Extensions.AI;

namespace HEAL.HeuristicAgent.Web.Services;

public sealed class LlmClient(
    IDataClient dataClient,
    IHeuristicChatClient chatClient,
    LlmResponseStream responseStream,
    IModelService modelService,
    IModelQualityService modelQualityService
) : IHostedService
{
    private const int NumValuesToUse = 20;

    private static readonly double QualityThreshold = 90.Percent;

    private readonly SemaphoreSlim _agentGate = new(1, 1);

    public bool AgentIsBusy => _agentGate.CurrentCount == 0;

    public Task StartAsync(CancellationToken ct)
    {
        dataClient.DataReceived += async (_, e) =>
        {
            var data = dataClient.Data;

            if (data.Count < NumValuesToUse)
            {
                return;
            }

            var recentData = data.TakeLast(NumValuesToUse).Select(x => x.Item2).ToArray();
            var combinedModel = await modelService.GetCombinedModelAsync();
            var quality = modelQualityService.EvaluateQuality(combinedModel, recentData);

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
