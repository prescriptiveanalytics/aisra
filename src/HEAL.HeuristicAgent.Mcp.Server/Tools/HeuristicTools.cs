using System.ComponentModel;
using HEAL.HeuristicLibAdapter;
using HEAL.HeuristicLibContracts.Dtos;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace HEAL.HeuristicAgent.Mcp.Server.Tools;

[McpServerToolType]
public sealed class HeuristicTools(IHeuristicLibClient client)
{
    [UsedImplicitly]
    [McpServerTool, Description("Runs a symbolic regression algorithm with the given hyperparameters and data.")]
    public async Task<CallToolResult> RunSymRegAsync(SymbolicRegressionHyperparametersDto parameters, CancellationToken ct = default)
    {
        try
        {
            var expression = await client.RunSymRegAsync(parameters, ct);

            return new CallToolResult
            {
                Content = [new TextContentBlock
                {
                    Text = expression
                }],
            };
        }
        catch (Exception ex)
        {
            return new CallToolResult
            {
                Content = [new TextContentBlock
                {
                    Text = $"ERROR: {ex.Message}"
                }],
                IsError = true,
            };
        }
    }
}
