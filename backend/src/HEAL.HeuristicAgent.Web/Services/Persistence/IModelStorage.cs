using HEAL.HeuristicAgent.Web.Dtos;

namespace HEAL.HeuristicAgent.Web.Services.Persistence;

public interface IModelStorage
{
    Task<int> SaveModelAsync(string model);
    Task<string?> GetResidualModelAsync(int id);
    IAsyncEnumerable<SymbolicRegressionModelDto> GetAllResidualModelsAsync();
    Task SaveBaseModelAsync(string model);
    Task<string?> GetBaseModelAsync();
}
