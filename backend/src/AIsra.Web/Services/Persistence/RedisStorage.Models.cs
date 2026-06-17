using AIsra.Web.Dtos;

namespace AIsra.Web.Services.Persistence;

partial class RedisStorage
{
    public async Task<int> SaveModelAsync(string model)
    {
        var id = await db.StringIncrementAsync("model-id");
        await db.StringSetAsync($"model:{id}", model);

        return (int)id;
    }

    public async Task<string?> GetResidualModelAsync(int id)
    {
        var model = await db.StringGetAsync($"model:{id}");
        return model.IsNullOrEmpty ? null : model.ToString();
    }

    public async IAsyncEnumerable<SymbolicRegressionModelDto> GetAllResidualModelsAsync()
    {
        var server = connection.GetServer(connection.GetEndPoints()[0]);

        await foreach (var key in server.KeysAsync(pattern: "model:*"))
        {
            var idStr = key.ToString()["model:".Length..];

            if (!int.TryParse(idStr, out var id))
            {
                continue;
            }

            var value = await db.StringGetAsync(key);

            if (!value.IsNullOrEmpty)
            {
                yield return new SymbolicRegressionModelDto
                {
                    Id = id,
                    Model = value.ToString(),
                };
            }
        }
    }

    public async Task SaveBaseModelAsync(string model)
    {
        await db.StringSetAsync("base-model", model);
        await db.StringSetAsync("model:0", "0");
    }

    public async Task<string?> GetBaseModelAsync()
    {
        var model = await db.StringGetAsync("base-model");
        return model.IsNullOrEmpty ? null : model.ToString();
    }
}
