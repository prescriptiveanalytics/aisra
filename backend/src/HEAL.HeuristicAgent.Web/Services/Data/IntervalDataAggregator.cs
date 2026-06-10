using System.Collections.Concurrent;
using HEAL.HeuristicAgent.Web.Dtos;
using HEAL.HeuristicAgent.Web.Services.Persistence;
using HEAL.HeuristicLibContracts.Threading;

namespace HEAL.HeuristicAgent.Web.Services.Data;

public sealed class IntervalDataAggregator : IDataAggregator
{
    private readonly ConcurrentDictionary<string, double> latestValues = new();

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

        Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                var delayTask = interval.WithCancellationToken(ct);

                var data = latestValues
                    .OrderBy(x => x.Key)
                    .Select(x => x.Value)
                    .ToArray();

                if (data.Length >= 3)
                {
                    await dataStorage.InsertAsync(data);
                    DataAggregated?.Invoke(this, data);
                }

                await delayTask;
            }
        }, ct);
    }

    public void Push(DataPointDto dataPoint)
        => latestValues[dataPoint.Id] = dataPoint.Value;

    public event EventHandler<double[]>? DataAggregated;
}
