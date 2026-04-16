using System.Net;
using HEAL.HeuristicLibWrapper.Dtos;
using HEAL.HeuristicLibWrapper.Exceptions;
using HEAL.HeuristicLibWrapper.Runners;
using HEAL.HeuristicRest.Dtos;
using HEAL.HeuristicRest.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace HEAL.HeuristicRest.Controllers;

[Controller]
[Route("benchmarks")]
public sealed class BenchmarkController(SolutionStore store) : ControllerBase
{
    private readonly TypedSolutionStore<double[]> _store = store.ToTyped<double[]>();

    [HttpPost("problems")]
    public ActionResult PostProblem([FromBody] FuncProblemDto dto)
    {
        try
        {
            var id = Guid.NewGuid();
            _store.Store(id);

            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await BenchmarkRunner.RunAsync(dto);

                    _store.Store(id, result, TrainingStatus.Successful);
                }
                catch (Exception)
                {
                    _store.Store(id, status: TrainingStatus.Failed);
                }
            });

            return AcceptedAtAction(nameof(GetStatus), new { id }, null);
        }
        catch (InvalidFunctionException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("status/{id:guid}")]
    public ActionResult<TrainingStatusDto> GetStatus(Guid id)
    {
        if (!_store.TryGet(id, out _, out var status))
        {
            return NotFound();
        }

        if (status == TrainingStatus.Successful)
        {
            HttpContext.Response.Headers.Location =
                Request.Scheme + "://" + Request.Host + Url.Action(nameof(GetSolution), new { id });
        }

        return Ok(new TrainingStatusDto(status.Value));
    }

    [HttpGet("solutions/{id:guid}")]
    public ActionResult<double[]> GetSolution(Guid id)
    {
        if (!_store.TryGet(id, out var solution, out var status))
        {
            return NotFound();
        }

        return status switch
        {
            TrainingStatus.Successful => Ok(solution),
            TrainingStatus.Running => BadRequest("Benchmark is still running."),
            _ => StatusCode((int)HttpStatusCode.InternalServerError, "Benchmark failed."),
        };
    }
}
