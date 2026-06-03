using HEAL.HeuristicAgent.Web.Dtos;
using StackExchange.Redis;

namespace HEAL.HeuristicAgent.Web.Services.Persistence;

/// <inheritdoc cref="IDataStorage" />
public sealed class RedisStorage : IDataStorage, IModelStorage, IDisposable, IAsyncDisposable
{
    private readonly ConnectionMultiplexer connection;
    private readonly IDatabase db;

    public RedisStorage(string host)
    {
        connection = ConnectionMultiplexer.Connect(host);
        db = connection.GetDatabase();
    }

    ~RedisStorage()
    {
        Dispose();
    }

    public async Task InsertAsync(double[] data)
    {
        await db.StreamAddAsync("data-records", [
            new("data", string.Join(",", data))
        ]);
    }

    public async IAsyncEnumerable<(DateTimeOffset, double[])> GetLastAsync(DateTimeOffset minTime)
    {
        var entries = await db.StreamRangeAsync(
            "data-records",
            messageOrder: Order.Descending,
            minId: $"{Math.Max(minTime.ToUnixTimeMilliseconds(), 0)}-0"
        );

        foreach (var entry in entries)
        {
            yield return (
                DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(entry.Id.ToString().Split('-')[0])),
                entry.Values[0].Value.ToString().Split(',').Select(double.Parse).ToArray()
            );
        }
    }

    public async IAsyncEnumerable<(DateTimeOffset, double[])> GetLastAsync(int count)
    {
        var entries = await db.StreamRangeAsync(
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
        var id = await db.StringIncrementAsync("model-id");
        await db.StringSetAsync($"model:{id}", model);

        return (int)id;
    }

    public async IAsyncEnumerable<SymbolicRegressionModelDto> GetAllResidualModelsAsync()
    {
        var id = (int)await db.StringGetAsync("model-id");

        for (var i = 1; i <= id; i++)
        {
            var model = await db.StringGetAsync($"model:{i}");

            if (!model.IsNullOrEmpty)
            {
                yield return new SymbolicRegressionModelDto(i, model.ToString());
            }
        }
    }

    public async Task SaveBaseModelAsync(string model)
    {
        await db.StringSetAsync("base-model", model);
    }

    public async Task<string?> GetBaseModelAsync()
    {
        var model = await db.StringGetAsync("base-model");
        return model.IsNullOrEmpty ? null : model.ToString();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await connection.DisposeAsync();
    }
}
