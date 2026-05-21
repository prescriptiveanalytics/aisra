using HEAL.HeuristicAgent.Web.Dtos;
using HEAL.HeuristicAgent.Web.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace HEAL.HeuristicAgent.Web.Controllers;

[ApiController]
[Route("models")]
public class ModelController(IModelStore modelStore) : ControllerBase
{
    [HttpGet]
    public IAsyncEnumerable<SymbolicRegressionModelDto> GetAllModels()
        => modelStore.GetAllModelsAsync();
}
