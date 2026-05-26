namespace HEAL.HeuristicLibContracts.Threading;

public interface ICancellationTokenProvider
{
    CancellationToken Token { get; }
}
