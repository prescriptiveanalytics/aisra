using System.Globalization;
using System.Net;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;
using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicLibContracts.Enums;
using HEAL.HeuristicLibWrapper.Exceptions;
using HEAL.HeuristicLibWrapper.Runners;
using HEAL.HeuristicWeb.Server.Rest.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace HEAL.HeuristicWeb.Server.Rest.Controllers;

[Controller]
[Route("symreg")]
public sealed class SymRegController(SolutionStore store) : ControllerBase
{
    private readonly TypedSolutionStore<SymbolicExpressionTree> _store = store.ToTyped<SymbolicExpressionTree>();

    /// <summary>
    /// Starts a new symbolic regression problem based on the provided parameters.
    /// The server responds right away, while the regression runs in the background.
    /// The client can GET the status of the algorithm at the location provided by the location header.
    /// </summary>
    [HttpPost("problems", Name = "PostSymRegProblem")]
    public ActionResult PostProblem([FromBody] SymbolicRegressionRequestDto dto)
    {
        try
        {
            var id = Guid.NewGuid();
            _store.Store(id);

            if (dto.Dataset.Data.Any(x => x.Length != dto.Dataset.VariableNames.Length))
            {
                return BadRequest("All data rows must have the same length as the variable names array.");
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await SymRegRunner.RunAsync(dto);

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
    /// Gets the status of a given symbolic regression problem, previously started by POST /symreg/problems.
    /// If the regression is successful, the location header is set to the URL where the solution can be retrieved.
    /// </summary>
    [HttpGet("status/{id:guid}", Name = "GetSymRegStatus")]
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
    /// Gets the solution of a successful regression algorithm, previously started by POST /symreg/problems.
    /// </summary>
    [HttpGet("solutions/{id:guid}", Name = "GetSymRegSolution")]
    public ActionResult<string> GetSolution(Guid id)
    {
        if (!_store.TryGet(id, out var solution, out var status))
        {
            return NotFound();
        }

        return status switch
        {
            TrainingStatus.Successful => Ok(InfixExpressionFormatter.Format(solution, NumberFormatInfo.InvariantInfo)),
            TrainingStatus.Running => BadRequest("Algorithm is still running."),
            _ => StatusCode((int)HttpStatusCode.InternalServerError, "Algorithm failed."),
        };
    }
}
