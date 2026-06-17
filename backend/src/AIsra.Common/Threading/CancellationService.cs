namespace AIsra.Common.Threading;

/// <inheritdoc cref="ICancellationTokenProvider" />
public sealed class CancellationService : ICancellationTokenProvider, IAsyncDisposable
{
    private readonly CancellationTokenSource cts = new();

    public CancellationToken Token => cts.Token;

    ~CancellationService()
    {
        cts.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await cts.CancelAsync();
        cts.Dispose();
    }
}
