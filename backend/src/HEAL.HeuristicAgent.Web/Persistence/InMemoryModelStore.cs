using HEAL.HeuristicAgent.Web.Dtos;

namespace HEAL.HeuristicAgent.Web.Persistence;

public sealed class InMemoryModelStore : IModelStore
{
    private readonly List<SymbolicRegressionModelDto> _models = new()
    {
        new(1, "0")
    };

    public Task<int> SaveModelAsync(string model)
    {
        _models.Add(new(_models.Count + 1, model));

        return Task.FromResult(_models.Count);
    }

    public Task<IReadOnlyCollection<SymbolicRegressionModelDto>> GetAllModelsAsync()
        => Task.FromResult<IReadOnlyCollection<SymbolicRegressionModelDto>>(_models);
}
