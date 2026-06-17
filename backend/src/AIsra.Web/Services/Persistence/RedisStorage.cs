using StackExchange.Redis;

namespace AIsra.Web.Services.Persistence;

/// <inheritdoc cref="IDataStorage" />
public sealed partial class RedisStorage : IDataStorage, IModelStorage, IDisposable, IAsyncDisposable
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
