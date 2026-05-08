namespace HEAL.HeuristicAgent.Web.Services;

public interface ICancellationTokenProvider
{
    CancellationToken Token { get; }
}
