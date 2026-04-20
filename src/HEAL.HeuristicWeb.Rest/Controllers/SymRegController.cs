using HEAL.HeuristicLibWrapper.Exceptions;
using HEAL.HeuristicLibWrapper.Runners;
using HEAL.HeuristicWeb.Rest.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace HEAL.HeuristicWeb.Rest.Controllers;

[Controller]
[Route("symreg")]
public sealed class SymRegController(SolutionStore store) : ControllerBase
{
    // private readonly TypedSolutionStore<>
    //
    // [HttpPost("problems")]
    // public ActionResult PostProblem([FromBody] SymRegDto dto)
    // {
    //     try
    //     {
    //         var id = Guid.NewGuid();
    //         SymRegRunner.RunAsync(new()).ContinueWith(task => store.Store(task.Result, id));
    //
    //         return AcceptedAtAction(nameof(GetSolution), new { id }, null);
    //     }
    //     catch (InvalidFunctionException ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    // }
    //
    // [HttpGet("solutions/{id:guid}")]
    // public ActionResult<double[]> GetSolution(Guid id)
    // {
    //     var solution = store.Get(id);
    //
    //     return solution is null ? NotFound() : solution;
    // }
}
