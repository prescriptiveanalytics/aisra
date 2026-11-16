using AIsra.Common.Threading;
using AIsra.Web.Dtos;
using AIsra.Web.Services.Chat;

namespace AIsra.Web.Services.Reset;

/// <summary>
/// Resets the entire application to the state it had right after starting.
/// </summary>
public sealed class ApplicationResetService(
    ILlmClient llmClient,
    IServiceScopeFactory scopeFactory,
    ApplicationEventStream eventStream,
    ICancellationTokenProvider ctp,
    ILogger<ApplicationResetService> logger
)
{
    private readonly SemaphoreSlim resetGate = new(1, 1);

    /// <remarks>
    /// The reset is not tied to the request that triggered it, because a partial reset would leave the application in
    /// an inconsistent state.
    /// </remarks>
    public async Task ResetAsync()
    {
        var ct = ctp.Token;

        await resetGate.WaitAsync(ct);

        try
        {
            // The agent must not run while its history, the data, or the models are being cleared.
            await using (await llmClient.SuspendAsync(ct))
            {
                // Resettables are resolved for every reset, because some of them are not singletons.
                await using var scope = scopeFactory.CreateAsyncScope();

                foreach (var resettable in scope.ServiceProvider.GetServices<IResettable>())
                {
                    await resettable.ResetAsync(ct);
                }
            }

            logger.LogInformation("The application was reset.");
        }
        finally
        {
            resetGate.Release();
        }

        // Lets every connected frontend discard its state as well.
        eventStream.Broadcast(EventType.Reset);
    }
}
