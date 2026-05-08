using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using HEAL.HeuristicAgent.Web.Dtos;
using HEAL.HeuristicAgent.Web.Services;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Problems.DataAnalysis;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression.Evaluators;
using HEAL.HeuristicLib.Problems.DataAnalysis.Symbolic;
using HEAL.HeuristicLibAdapter;
using HEAL.HeuristicLibContracts.Dtos;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace HEAL.HeuristicAgent.Web.Mcp.Server.Tools;

[McpServerToolType]
public sealed class HeuristicTools(IHeuristicLibClient client, IDataClient dataRouter, LlmResponseStream responseStream)
{
    private static readonly InfixExpressionParser Parser = new();

    private static readonly SymbolicExpressionTree BaseModel =
        Parser.Parse("'x0' * 'x0' + 'x1' / 2 + 7");

    private static SymbolicExpressionTree ResidualModel
    {
        get;
        set
        {
            field = value;
            CombinedModel =
                Parser.Parse(
                    $"({InfixExpressionFormatter.Format(BaseModel, NumberFormatInfo.InvariantInfo)})"
                    + $" + ({InfixExpressionFormatter.Format(field, NumberFormatInfo.InvariantInfo)})"
                );
        }
    } = Parser.Parse("0");

    public static SymbolicExpressionTree CombinedModel { get; private set; } =
        Parser.Parse(
            $"({InfixExpressionFormatter.Format(BaseModel, NumberFormatInfo.InvariantInfo)})"
            + $" + ({InfixExpressionFormatter.Format(ResidualModel, NumberFormatInfo.InvariantInfo)})"
        );

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    [UsedImplicitly]
    [McpServerTool]
    [Description(
        "Runs a symbolic regression algorithm with the given hyperparameters and data and updates the current model."
    )]
    public Task<CallToolResult> RunSymbolicRegressionAsync(
        CancellationToken ct = default
    ) => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Running symbolic regression");
        var instructions = new SymbolicRegressionInstructionsDto
        {
            StartTimeIncl = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1),
        };

        var data = dataRouter.Data
            .Where(data =>
                data.Item1 >= instructions.StartTimeIncl
                && (
                    instructions.EndTimeExcl is null
                    || data.Item1 <= instructions.EndTimeExcl
                )
            )
            .Select(d => new DataDto
            {
                Timestamp = d.Item1,
                Data = d.Item2,
            })
            .ToArray();

        if (data.Length == 0)
        {
            throw new ArgumentException("No data available in the specified range.");
        }

        var datasetRows = data.Select(d => d.Data.ToArray()).ToArray();
        var variableNames = Enumerable.Range(0, datasetRows.First().Length - 1).Select(i => $"x{i}").Append("y")
            .ToArray();

        // Calculate residuals using BaseModel
        var baseModelEvaluator = new SymbolicRegressionModel(
            BaseModel,
            new SymbolicDataAnalysisExpressionTreeInterpreter()
        );
        var hlDataset = Dataset.FromRowData(variableNames, datasetRows);
        var basePredictions = baseModelEvaluator.Predict(hlDataset, Enumerable.Range(0, hlDataset.Rows)).ToArray();

        // Modify the dataset to contain residuals
        for (var i = 0; i < datasetRows.Length; i++)
        {
            var trueY = datasetRows[i].Last();
            var predictedY = basePredictions[i];
            var residual = trueY - predictedY;

            var newRow = new double[datasetRows[i].Length];
            var length = datasetRows[i].Length;
            var col = 0;
            foreach (var val in datasetRows[i])
            {
                newRow[col++] = val;
            }

            newRow[length - 1] = residual;
            datasetRows[i] = newRow;
        }

        var request = new SymbolicRegressionRequestDto
        {
            Hyperparameters = instructions.Hyperparameters,
            Dataset = new SymbolicRegressionDatasetDto
            {
                Data = datasetRows,
                VariableNames = variableNames,
                TargetVariableName = "y",
            },
        };

        var expression = await client.RunSymRegAsync(request, ct);
        ResidualModel = Parser.Parse(expression);

        responseStream.Broadcast(EventType.Tool, "Symbolic regression completed");

        return new CallToolResult
        {
            Content =
            [
                new TextContentBlock
                {
                    Text = expression,
                },
            ],
        };
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Retrieves the latest data for symbolic regression.")]
    public CallToolResult GetData(int numRows) => Do(() =>
    {
        responseStream.Broadcast(EventType.Tool, "Retrieving data");

        return new CallToolResult
        {
            Content =
            [
                new TextContentBlock
                {
                    Text =
                        JsonSerializer.Serialize(
                            dataRouter.Data.Select(d => new DataDto
                                {
                                    Timestamp = d.Item1,
                                    Data = d.Item2,
                                })
                                .OrderByDescending(d => d.Timestamp)
                                .Take(numRows)
                                .ToArray(),
                            JsonOptions
                        ),
                }
            ]
        };
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets the quality of the current model on the data starting from the specified time.")]
    public CallToolResult GetModelQuality(DateTimeOffset startTimeIncl) => Do(() =>
    {
        responseStream.Broadcast(EventType.Tool, "Evaluating model quality");

        var data = dataRouter.Data
            .Where(d => d.Item1 >= startTimeIncl)
            .Select(d => d.Item2)
            .ToArray();

        if (data.Length == 0)
        {
            throw new ArgumentException("No data available in the specified range.");
        }

        var variableNames = Enumerable.Range(0, data.First().Length - 1)
            .Select(i => $"x{i}")
            .Append("y")
            .ToArray();
        var dataset = Dataset.FromRowData(variableNames, data);
        var model = new SymbolicRegressionModel(
            CombinedModel,
            new SymbolicDataAnalysisExpressionTreeInterpreter()
        );
        var predictions = model.Predict(dataset, Enumerable.Range(0, dataset.Rows)).ToArray();
        var trueValues = data.Select(d => d.Last()).ToArray();

        var quality = new PearsonR2Evaluator().Evaluate(predictions, trueValues);

        return new CallToolResult
        {
            Content =
            [
                new TextContentBlock
                {
                    Text = $"{quality:F4}"
                },
            ],
        };
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets the currently used expression")]
    public CallToolResult GetCurrentModel() => Do(() =>
    {
        responseStream.Broadcast(EventType.Tool, "Retrieving current model");

        return new CallToolResult
        {
            Content =
            [
                new TextContentBlock
                {
                    Text = InfixExpressionFormatter.Format(CombinedModel, NumberFormatInfo.InvariantInfo)
                },
            ],
        };
    });

    private static CallToolResult Do(Func<CallToolResult> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in tool execution: {ex}");

            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock
                    {
                        Text = $"ERROR: {ex.Message}"
                    },
                ],
                IsError = true,
            };
        }
    }

    private static async Task<CallToolResult> DoAsync(Func<Task<CallToolResult>> func)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in tool execution: {ex}");

            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock
                    {
                        Text = $"ERROR: {ex.Message}"
                    },
                ],
                IsError = true,
            };
        }
    }
}
