using HEAL.HeuristicAgent.Web.Dtos;

namespace HEAL.HeuristicAgent.Web.Services.Persistence;

public sealed class InMemoryModelStorage : IModelStorage
{
    private string? baseModel;

    private readonly List<SymbolicRegressionModelDto> models = new()
    {
        new(1, "0"),
    };

    public Task SaveBaseModelAsync(string model)
    {
        baseModel = model;

        return Task.CompletedTask;
    }

    public Task<string?> GetBaseModelAsync()
    {
        return Task.FromResult(baseModel);
    }

    public Task<int> SaveModelAsync(string model)
    {
        models.Add(new SymbolicRegressionModelDto(models.Count + 1, model));

        return Task.FromResult(models.Count);
    }

    public IAsyncEnumerable<SymbolicRegressionModelDto> GetAllResidualModelsAsync()
        => models.ToAsyncEnumerable();
}
