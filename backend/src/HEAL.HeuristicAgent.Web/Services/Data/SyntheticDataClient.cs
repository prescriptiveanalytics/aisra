using HEAL.HeuristicAgent.Web.Services.Persistence;
using HEAL.HeuristicLibContracts.Threading;

namespace HEAL.HeuristicAgent.Web.Services.Data;

public sealed class SyntheticDataClient : IDataClient
{
    private const int MinValue = -100;
    private const int MaxValue = 100;

    public event EventHandler<double[]>? DataReceived;

    private static readonly Func<double, double, double> F1 = (x1, x2) => x1 * x1 * (1 + x1 / 2000) + x2 * x2 / 1.99 + 7.01;
    private static readonly Func<double, double, double> F2 = (x1, x2) => x1 * x1 * (1 + x1 / 200) + x2 * x2 / 1.8 + 7.2;

    private bool useF2;
    private int i;
    private int interval = 200;

    public SyntheticDataClient(IDataStore dataStore, ICancellationTokenProvider ctp)
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
                    x1, x2, useF2 ? F2(x1, x2) : F1(x1, x2)
                };

                await dataStore.InsertAsync(data);

                DataReceived?.Invoke(this, data);

                if (++i % interval == 0)
                {
                    useF2 = !useF2;
                    // _interval *= 12;
                    interval = int.MaxValue;
                }

                await delayTask;
            }
        }, ct);
    }
}
