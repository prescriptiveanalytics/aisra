using AIsra.Web.Services.Reset;
using StackExchange.Redis;

namespace AIsra.Web.Services.Persistence;

/// <inheritdoc cref="IDataStorage" />
public sealed partial class RedisStorage : IDataStorage, IModelStorage, IResettable, IDisposable, IAsyncDisposable
{
    private const string DataRecordsKey = "data-records";
    private const string ModelIdKey = "model-id";
    private const string BaseModelKey = "base-model";
    private const string ModelKeyPrefix = "model:";

    private readonly ConnectionMultiplexer connection;
    private readonly IDatabase db;

    public RedisStorage(string host)
    {
        connection = ConnectionMultiplexer.Connect(host);
        db = connection.GetDatabase();
    }

    /// <summary>
    /// Deletes all data and models.
    /// </summary>
    public async Task ResetAsync(CancellationToken ct)
    {
        await db.KeyDeleteAsync([DataRecordsKey, ModelIdKey, BaseModelKey]);

        var server = connection.GetServer(connection.GetEndPoints()[0]);
        var modelKeys = await server.KeysAsync(pattern: ModelKeyPrefix + "*").ToArrayAsync(ct);

        if (modelKeys.Length > 0)
        {
            await db.KeyDeleteAsync(modelKeys);
        }
    }

    ~RedisStorage()
    {
        Dispose();
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
