using HEAL.HeuristicAgent.Web.Dtos;

namespace HEAL.HeuristicAgent.Web.Services.Persistence;

public interface IModelStorage
{
    Task<int> SaveModelAsync(string model);
    IAsyncEnumerable<SymbolicRegressionModelDto> GetAllResidualModelsAsync();
    Task SaveBaseModelAsync(string model);
    Task<string?> GetBaseModelAsync();
}
