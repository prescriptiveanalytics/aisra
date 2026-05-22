using HEAL.HeuristicAgent.Web.Dtos;
using StackExchange.Redis;

namespace HEAL.HeuristicAgent.Web.Persistence;

public sealed class RedisStore : IDataStore, IModelStore, IDisposable, IAsyncDisposable
{
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _db;

    public RedisStore(string host)
    {
        _connection = ConnectionMultiplexer.Connect(host);
        _db = _connection.GetDatabase();
    }

    ~RedisStore()
    {
        Dispose();
    }

    public async Task InsertAsync(double[] data)
    {
        await _db.StreamAddAsync("data-records", [
            new("data", string.Join(",", data))
        ]);
    }

    public async IAsyncEnumerable<(DateTimeOffset, double[])> GetLastAsync(DateTimeOffset minTime)
    {
        const int batchSize = 100;

        StreamEntry[] entries;

        do
        {
            entries = await _db.StreamRangeAsync(
                "data-records",
                count: batchSize,
                messageOrder: Order.Descending,
                minId: $"{minTime.ToUnixTimeMilliseconds()}-0"
            );

            foreach (var entry in entries)
            {
                yield return (
                    DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(entry.Id.ToString().Split('-')[0])),
                    entry.Values[0].Value.ToString().Split(',').Select(double.Parse).ToArray()
                );
            }
        } while (entries.Length > 0);
    }

    public async IAsyncEnumerable<(DateTimeOffset, double[])> GetLastAsync(int count)
    {
        var entries = await _db.StreamRangeAsync(
            "data-records",
            count: count,
            messageOrder: Order.Descending
        );

        foreach (var entry in entries)
        {
            yield return (
                DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(entry.Id.ToString().Split('-')[0])),
                entry.Values[0].Value.ToString().Split(',').Select(double.Parse).ToArray()
            );
        }
    }

    public async Task<int> SaveModelAsync(string model)
    {
        var id = await _db.StringIncrementAsync("model-id");
        await _db.StringSetAsync($"model:{id}", model);

        return (int)id;
    }

    public async IAsyncEnumerable<SymbolicRegressionModelDto> GetAllModelsAsync()
    {
        var id = (int)await _db.StringGetAsync("model-id");

        for (var i = 1; i <= id; i++)
        {
            var model = await _db.StringGetAsync($"model:{i}");

            if (!model.IsNullOrEmpty)
            {
                yield return new SymbolicRegressionModelDto(i, model.ToString());
            }
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await _connection.DisposeAsync();
    }
}
