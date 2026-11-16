using AIsra.Web.Dtos;
using AIsra.Web.Services.Data;
using AIsra.Web.Services.Modeling;
using AIsra.Web.Services.Persistence;
using AIsra.Web.Services.Reset;
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
    Settings settings,
    ICancellationTokenProvider ctp,
    ILogger<LlmClient> logger
) : ILlmClient, IResettable
{
    private const int NumValuesToUse = 20; // TODO: Make this configurable

    private static readonly double QualityThreshold = 70.Percent; // TODO: Make this configurable

    /// <summary>
    /// The time after an interruption during which a quality drop does not prompt the agent.
    /// </summary>
    private static readonly TimeSpan InterruptCooldown = TimeSpan.FromSeconds(10);

    private readonly SemaphoreSlim agentGate = new(1, 1);
    private readonly Lock runLock = new();
    private CancellationTokenSource? currentRun;
    private DateTimeOffset cooldownUntil = DateTimeOffset.MinValue;

    public bool AgentIsBusy => agentGate.CurrentCount == 0;

    public Task StartAsync(CancellationToken ct)
    {
        dataClient.DataReceived += async (_, _) =>
        {
            try
            {
                await HandleDataReceivedAsync(ct);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to check the model quality.");
            }
        };

        return Task.CompletedTask;
    }

    private async Task HandleDataReceivedAsync(CancellationToken ct)
    {
        if (!settings.MonitorModelQuality || DateTimeOffset.UtcNow < cooldownUntil || AgentIsBusy)
        {
            return;
        }

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

        if (quality > QualityThreshold || !settings.MonitorModelQuality || DateTimeOffset.UtcNow < cooldownUntil)
        {
            return;
        }

        await RunAgentAsync(
            new ChatMessage(
                ChatRole.User,
                $"[{DateTimeOffset.UtcNow}] [SYSTEM]" +
                $" The model quality fell below the threshold of {QualityThreshold} (quality: {quality:F2})."
            ),
            () => responseStream.Broadcast(EventType.QualityDrop)
        );
    }

    public Task ChatAsync(string message)
        => RunAgentAsync(new ChatMessage(ChatRole.User, message));

    public bool Interrupt()
    {
        lock (runLock)
        {
            cooldownUntil = DateTimeOffset.UtcNow + InterruptCooldown;

            if (currentRun is null)
            {
                return false;
            }

            currentRun.Cancel();
            return true;
        }
    }

    public async Task<IAsyncDisposable> SuspendAsync(CancellationToken ct)
    {
        lock (runLock)
        {
            currentRun?.Cancel();
        }

        await agentGate.WaitAsync(ct);

        return new Suspension(agentGate);
    }

    public Task ResetAsync(CancellationToken ct)
    {
        lock (runLock)
        {
            cooldownUntil = DateTimeOffset.MinValue;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Prompts the agent with the given message unless it is already busy, and streams its response to the frontend.
    /// </summary>
    /// <param name="message">The message to send to the agent.</param>
    /// <param name="onStart">Called once the agent has been acquired, before it is prompted.</param>
    private async Task RunAgentAsync(ChatMessage message, Action? onStart = null)
    {
        if (!await agentGate.WaitAsync(0))
        {
            return;
        }

        using var run = CancellationTokenSource.CreateLinkedTokenSource(ctp.Token);

        lock (runLock)
        {
            currentRun = run;
        }

        var finalEvent = EventType.Done;

        try
        {
            responseStream.Broadcast(EventType.Started);
            onStart?.Invoke();

            await foreach (var s in chatClient.GetStreamingResponseAsync([message], run.Token))
            {
                responseStream.Broadcast(EventType.Fragment, s);
            }
        }
        catch (OperationCanceledException) when (run.IsCancellationRequested)
        {
            finalEvent = EventType.Interrupted;
        }
        catch (Exception e)
        {
            logger.LogError(e, "The agent failed to respond.");
        }
        finally
        {
            lock (runLock)
            {
                currentRun = null;
            }

            agentGate.Release();
            responseStream.Broadcast(finalEvent);
        }
    }

    public Task StopAsync(CancellationToken ct)
        => Task.CompletedTask;

    private sealed class Suspension(SemaphoreSlim agentGate) : IAsyncDisposable
    {
        private int disposed;

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                agentGate.Release();
            }

            return ValueTask.CompletedTask;
        }
    }
}
