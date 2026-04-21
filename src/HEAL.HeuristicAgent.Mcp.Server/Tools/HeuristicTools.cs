using System.ComponentModel;
using HEAL.HeuristicLibAdapter;
using HEAL.HeuristicLibContracts.Dtos;
using ModelContextProtocol.Server;

namespace HEAL.HeuristicAgent.Mcp.Server.Tools;

[McpServerToolType]
public sealed class HeuristicTools(IHeuristicLibClient client)
{
    [McpServerTool, Description("Runs a symbolic regression algorithm with the given hyperparameters and data.")]
    public async Task<string> RunSymRegAsync(SymRegProblemDto parameters, CancellationToken ct = default)
        => await client.RunSymRegAsync(parameters, ct);
}
