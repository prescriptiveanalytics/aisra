using System.Net;
using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicLibContracts.Enums;
using HEAL.HeuristicLibWrapper.Exceptions;
using HEAL.HeuristicLibWrapper.Runners;
using HEAL.HeuristicWeb.Server.Rest.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace HEAL.HeuristicWeb.Server.Rest.Controllers;

[Controller]
[Route("benchmarks")]
public sealed class BenchmarksController(SolutionStore store) : ControllerBase
{
    private readonly TypedSolutionStore<double[]> _store = store.ToTyped<double[]>();

    /// <summary>
    /// Starts a new benchmark based on the provided parameters.
    /// The server responds right away, while the benchmark runs in the background.
    /// The client can GET the status of the benchmark at the location provided by the location header.
    /// </summary>
    [HttpPost("problems", Name = "PostBenchmarkProblem")]
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

    /// <summary>
    /// Gets the status of a given benchmark, previously started by POST /benchmarks/problems.
    /// If the benchmark is successful, the location header is set to the URL where the solution can be retrieved.
    /// </summary>
    [HttpGet("status/{id:guid}", Name = "GetBenchmarkStatus")]
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

    /// <summary>
    /// Gets the solution of a successful benchmark, previously started by POST /benchmarks/problems.
    /// </summary>
    [HttpGet("solutions/{id:guid}", Name = "GetBenchmarkSolution")]
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
