using HEAL.HeuristicAgent.Web.Dtos;
using StackExchange.Redis;

namespace HEAL.HeuristicAgent.Web.Persistence;

public sealed class RedisModelStore : IModelStore, IDisposable, IAsyncDisposable
{
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _db;

    public RedisModelStore(string host)
    {
        _connection = ConnectionMultiplexer.Connect(host);
        _db = _connection.GetDatabase();
    }

    ~RedisModelStore()
    {
        Dispose();
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
