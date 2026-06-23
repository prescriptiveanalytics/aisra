using System.Net;
using AIsra.Common.Dtos;
using AIsra.Common.Enums;
using AIsra.Common.Random;
using AIsra.HeuristicLibAdapter.Lib.Exceptions;
using AIsra.HeuristicLibAdapter.Lib.Runners;
using AIsra.HeuristicLibWeb.Server.Rest.Storage;
using Microsoft.AspNetCore.Mvc;

namespace AIsra.HeuristicLibWeb.Server.Rest.Controllers;

[Controller]
[Route("benchmarks")]
public sealed class BenchmarksController(SolutionStorage storage, IRng rng) : ControllerBase
{
    private readonly TypedSolutionStorage<double[]> storage = storage.ToTyped<double[]>();

    /// <summary>
    /// Starts a new benchmark based on the provided parameters.
    /// The server responds right away, while the benchmark runs in the background.
    /// The client can GET the status of the benchmark at the location provided by the location header.
    /// </summary>
    [HttpPost("problems", Name = "PostBenchmarkProblem")]
    public ActionResult PostProblem([FromBody] BenchmarkHyperparametersDto dto, CancellationToken ct = default)
    {
        try
        {
            var id = Guid.NewGuid();
            storage.Store(id);

            Task.Run(async () =>
            {
                try
                {
                    var result = await BenchmarkRunner.RunAsync(dto, rng.Next(), ct);

                    storage.Store(id, result, TrainingStatus.Successful);
                }
                catch (Exception)
                {
                    storage.Store(id, status: TrainingStatus.Failed);
                }
            }, ct);

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
        if (!storage.TryGet(id, out _, out var status))
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
        if (!storage.TryGet(id, out var solution, out var status))
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
