namespace HEAL.HeuristicLibContracts.Threading;

/// <summary>
/// Provides a single <see cref="CancellationToken"/> for all services.
/// </summary>
public interface ICancellationTokenProvider
{
    CancellationToken Token { get; }
}
