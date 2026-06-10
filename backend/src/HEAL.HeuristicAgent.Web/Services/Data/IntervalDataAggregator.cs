using HEAL.HeuristicAgent.Web.Dtos;
using HEAL.HeuristicAgent.Web.Services.Persistence;
using HEAL.HeuristicLibContracts.Threading;

namespace HEAL.HeuristicAgent.Web.Services.Data;

public sealed class IntervalDataAggregator : IDataAggregator
{
    private readonly SortedDictionary<string, double> latestValues = new();
    private readonly Lock locker = new();

    public IntervalDataAggregator(
        IConfiguration config,
        IDataStorage dataStorage,
        ICancellationTokenProvider ctp
    )
    {
        var interval = config["DataAggregationIntervalMs"] is not null
            ? int.Parse(config["DataAggregationIntervalMs"]!).Milliseconds
            : 500.Milliseconds;
        var ct = ctp.Token;

        Task.Factory.StartNew(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                var delayTask = interval.WithCancellationToken(ct);

                double[] data;
                lock (locker)
                {
                    data = latestValues.Values.ToArray();
                }

                if (data.Length >= 3)
                {
                    await dataStorage.InsertAsync(data);
                    DataAggregated?.Invoke(this, data);
                }

                await delayTask;
            }
        }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    }

    public void Push(DataPointDto dataPoint)
    {
        lock (locker)
        {
            latestValues[dataPoint.Id] = dataPoint.Value;
        }
    }

    public event EventHandler<double[]>? DataAggregated;
}
