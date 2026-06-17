using AIsra.Web.Dtos;
using AIsra.Web.Services.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace AIsra.Web.Controllers;

[ApiController]
[Route("models")]
public class ModelController(IModelStorage modelStore) : ControllerBase
{
    [HttpGet]
    public IAsyncEnumerable<SymbolicRegressionModelDto> GetAllModels()
        => modelStore.GetAllResidualModelsAsync();
}
