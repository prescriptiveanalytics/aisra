using HEAL.HeuristicAgent.Web.Persistence;
using HEAL.HeuristicLibContracts.Threading;

namespace HEAL.HeuristicAgent.Web.Services;

public interface IDataClient
{
    event EventHandler<double[]> DataReceived;
}

public sealed class DataClient : IDataClient
{
    private const int MinValue = -100;
    private const int MaxValue = 100;

    public event EventHandler<double[]>? DataReceived;

    private static readonly Func<double, double, double> F1 = (x1, x2) => x1 * x1 * (1 + x1 / 2000) + x2 * x2 / 1.99 + 7.01;
    private static readonly Func<double, double, double> F2 = (x1, x2) => x1 * x1 * (1 + x1 / 200) + x2 * x2 / 1.8 + 7.2;

    private bool _useF2;
    private int _i;
    private int _interval = 40;

    public DataClient(IDataStore dataStore, ICancellationTokenProvider ctp)
    {
        var ct = ctp.Token;

        Task.Run(async () =>
        {
            var rand = new Random();

            while (!ct.IsCancellationRequested)
            {
                var delayTask = 0.5.Seconds.WithCancellationToken(ct);

                var x1 = rand.NextDouble() * (MaxValue - MinValue) + MinValue;
                var x2 = rand.NextDouble() * (MaxValue - MinValue) + MinValue;

                var data = new[]
                {
                    x1, x2, _useF2 ? F2(x1, x2) : F1(x1, x2)
                };

                await dataStore.InsertAsync(data);

                DataReceived?.Invoke(this, data);

                if (++_i % _interval == 0)
                {
                    _useF2 = !_useF2;
                    _interval *= 12;
                }

                await delayTask;
            }
        }, ct);
    }
}
