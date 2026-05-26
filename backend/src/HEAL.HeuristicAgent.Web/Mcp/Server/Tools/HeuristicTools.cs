using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using HEAL.HeuristicAgent.Web.Dtos;
using HEAL.HeuristicAgent.Web.Persistence;
using HEAL.HeuristicAgent.Web.Services;
using HEAL.HeuristicLib.Problems.DataAnalysis;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression;
using HEAL.HeuristicLib.Problems.DataAnalysis.Symbolic;
using HEAL.HeuristicLibAdapter;
using HEAL.HeuristicLibContracts.Dtos;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace HEAL.HeuristicAgent.Web.Mcp.Server.Tools;

[McpServerToolType]
public sealed class HeuristicTools(
    IHeuristicLibClient client,
    LlmResponseStream responseStream,
    IModelStore modelStore,
    IDataStore dataStore,
    IModelService modelService,
    IModelAnalysisService modelAnalysisService
)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    [UsedImplicitly]
    [McpServerTool]
    [Description(
        "Runs a symbolic regression algorithm with the given data to create and store a new model. " +
        "Does not activate the generated model automatically. " +
        "Returns the generated expression and the ID of the stored model."
    )]
    public Task<CallToolResult> RunSymbolicRegressionAsync(
        [Description("The start time (inclusive) for the data to use in symbolic regression")]
        DateTimeOffset startTimeIncl,
        CancellationToken ct = default
    ) => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Running symbolic regression");

        var instructions = new SymbolicRegressionInstructionsDto
        {
            StartTimeIncl = startTimeIncl,
        };

        var data = await dataStore.GetLastAsync(startTimeIncl)
            .Select(d => new DataDto
            {
                Timestamp = d.Item1,
                Data = d.Item2,
            })
            .ToArrayAsync(cancellationToken: ct);

        if (data.Length == 0)
        {
            throw new ArgumentException("No data available in the specified range.");
        }

        var datasetRows = data.Select(d => d.Data.ToArray()).ToArray();
        var variableNames = Enumerable.Range(1, datasetRows[0].Length - 1)
            .Select(i => $"x{i}")
            .Append("y")
            .ToArray();

        // Calculate residuals using BaseModel
        var baseModelEvaluator = new SymbolicRegressionModel(
            modelService.GetBaseModel(),
            new SymbolicDataAnalysisExpressionTreeInterpreter()
        );
        var dataset = Dataset.FromRowData(variableNames, datasetRows);
        var basePredictions = baseModelEvaluator.Predict(dataset, Enumerable.Range(0, dataset.Rows)).ToArray();

        // Modify the dataset to contain residuals
        for (var i = 0; i < datasetRows.Length; i++)
        {
            var trueY = datasetRows[i][^1];
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
        
        var id = await modelStore.SaveModelAsync(expression);

        responseStream.Broadcast(EventType.Tool, "Symbolic regression completed");

        return TextResult(JsonSerializer.Serialize(
            new
            {
                ModelId = id,
                Expression = expression,
            },
            JsonOptions
        ));
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets the quality of a model over time.")]
    public Task<CallToolResult> GetModelQualityOverTime(
        [Description("The start time (inclusive) for the data to evaluate the model on")]
        DateTimeOffset startTimeIncl,
        [Description("The number of data points to include in each quality data point")]
        int windowSize,
        [Description("The ID of the model to evaluate, or the active one if not provided")]
        int? modelId = null,
        CancellationToken ct = default
    ) => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Evaluating model quality over time");

        var data = await dataStore.GetLastAsync(startTimeIncl)
            .Where(d => d.Item1 >= startTimeIncl)
            .Select(d => (Timestamp: d.Item1, Values: d.Item2))
            .ToArrayAsync(cancellationToken: ct);

        if (data.Length == 0)
        {
            throw new ArgumentException("No data available in the specified range.");
        }

        var combinedModel = await modelService.GetCombinedModelAsync(modelId);
        var valuesOnly = data.Select(d => d.Values).ToArray();
        
        var qualityList = modelAnalysisService.EvaluateQualityOverTime(combinedModel, valuesOnly, windowSize);

        var qualityOverTime = data.Zip(qualityList, (d, q) => new
        {
            d.Timestamp,
            Quality = q
        });

        return TextResult(JsonSerializer.Serialize(qualityOverTime, JsonOptions));
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets the base model (the original model without any residuals)")]
    public CallToolResult GetBaseModel() => Do(() =>
    {
        responseStream.Broadcast(EventType.Tool, "Retrieving base model");

        return TextResult(InfixExpressionFormatter.Format(modelService.GetBaseModel(), NumberFormatInfo.InvariantInfo));
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets the residual model by ID")]
    public Task<CallToolResult> GetResidualModel(
        [Description("The ID of the model to retrieve, or the active one if not provided")]
        int? modelId = null
    ) => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Retrieving residual model");

        var residualModel = await modelService.GetResidualModelAsync(modelId);
        return TextResult(InfixExpressionFormatter.Format(residualModel, NumberFormatInfo.InvariantInfo));
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets the combined model (base + residual) by ID")]
    public Task<CallToolResult> GetCombinedModel(
        [Description("The ID of the model to retrieve, or the active one if not provided")]
        int? modelId = null
    ) => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Retrieving combined model");

        var combinedModel = await modelService.GetCombinedModelAsync(modelId);
        return TextResult(InfixExpressionFormatter.Format(combinedModel, NumberFormatInfo.InvariantInfo));
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets all saved models in the model store")]
    public Task<CallToolResult> GetAllSavedModelsAsync() => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Retrieving all saved models");

        var models = await modelStore.GetAllModelsAsync().ToArrayAsync();

        return TextResult(JsonSerializer.Serialize(models, JsonOptions));
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description(
        "Retrieves the model with the given ID from the model store and sets it as the active model used for predictions"
    )]
    public Task<CallToolResult> SetActiveModel(int modelId) => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, $"Switching active model to ID {modelId}");

        var models = modelStore.GetAllModelsAsync();

        if (await models.AllAsync(m => m.Id != modelId))
        {
            throw new ArgumentException($"Model with ID {modelId} not found.");
        }

        modelService.SetActiveModel(modelId);

        return TextResult($"Active model set to ID {modelId}");
    });

    private static CallToolResult Do(Func<CallToolResult> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return CreateErrorResult(ex);
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
            return CreateErrorResult(ex);
        }
    }

    private static CallToolResult CreateErrorResult(Exception ex)
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

    private static CallToolResult TextResult(string text) => new()
    {
        Content =
        [
            new TextContentBlock
            {
                Text = text,
            },
        ],
    };
}
