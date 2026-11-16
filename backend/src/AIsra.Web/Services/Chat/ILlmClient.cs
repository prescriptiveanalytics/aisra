namespace AIsra.Web.Services.Chat;

public interface ILlmClient : IHostedService
{
    bool AgentIsBusy { get; }

    Task ChatAsync(string message);

    /// <summary>
    /// Interrupts the agent if it is currently responding, and starts a cooldown during which a quality drop does
    /// not prompt the agent.
    /// </summary>
    /// <returns><see langword="true"/> if a running response was interrupted.</returns>
    bool Interrupt();

    /// <summary>
    /// Interrupts the agent if it is currently responding, waits until it has stopped, and keeps it from being
    /// prompted until the returned handle is disposed.
    /// </summary>
    Task<IAsyncDisposable> SuspendAsync(CancellationToken ct);
}
