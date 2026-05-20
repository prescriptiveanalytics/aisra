namespace HEAL.HeuristicAgent.Web.Services;

public interface IDataClient
{
    IReadOnlyCollection<(DateTimeOffset, double[])> Data { get; }

    event EventHandler<double[]> DataReceived;
}

public sealed class DataClient : IDataClient
{
    private const int MinValue = -100;
    private const int MaxValue = 100;

    private readonly Queue<(DateTimeOffset, double[])> _data = new();

    public event EventHandler<double[]>? DataReceived;

    private static readonly Func<double, double, double> F1 = (x1, x2) => x1 * x1 * (1 + x1 / 2000) + x2 / 1.99 + 7.01;
    private static readonly Func<double, double, double> F2 = (x1, x2) => x1 * x1 * (1 + x1 / 300) + x2 / 1.88 + 7.11;

    private bool _useF2;
    private int _i;
    private int _interval = 40;

    public DataClient(ICancellationTokenProvider ctp)
    {
        var ct = ctp.Token;

        Task.Run(async () =>
        {
            var rand = new Random();
            while (!ct.IsCancellationRequested)
            {
                var x1 = rand.NextDouble() * (MaxValue - MinValue) + MinValue;
                var x2 = rand.NextDouble() * (MaxValue - MinValue) + MinValue;

                var data = new[]
                {
                    x1, x2, _useF2 ? F2(x1, x2) : F1(x1, x2)
                };

                _data.Enqueue((DateTimeOffset.UtcNow, data));

                DataReceived?.Invoke(this, data);

                await 0.5.Seconds.WithCancellationToken(ct);

                if (++_i % _interval != 0)
                {
                    continue;
                }

                _useF2 = !_useF2;
                _interval *= 12;
            }
        }, ct);
    }

    public IReadOnlyCollection<(DateTimeOffset, double[])> Data => _data;
}
