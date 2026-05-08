namespace HEAL.HeuristicAgent.Web.Services;

public sealed class CancellationService : ICancellationTokenProvider, IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();

    public CancellationToken Token => _cts.Token;

    ~CancellationService()
    {
        _cts.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}
