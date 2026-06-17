using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using AIsra.Web.Dtos;
using AIsra.Web.Services.Chat;
using AIsra.Web.Services.Modeling;
using AIsra.Web.Services.Persistence;
using HEAL.HeuristicLib.Problems.DataAnalysis;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression;
using HEAL.HeuristicLib.Problems.DataAnalysis.Symbolic;
using AIsra.HeuristicLibAdapter;
using AIsra.Common.Dtos;
using JetBrains.Annotations;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AIsra.Web.Services.Mcp.Server;

[McpServerToolType]
public sealed partial class HeuristicTools(
    IHeuristicLibClient client,
    ApplicationEventStream responseStream,
    IModelStorage modelStorage,
    IDataStorage dataStorage,
    IModelService modelService,
    IModelAnalyzer modelAnalyzer,
    Settings features
)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    private static readonly SymbolicDataAnalysisExpressionTreeInterpreter Interpreter = new();

    [UsedImplicitly]
    [McpServerTool]
    [Description(
        "Runs a symbolic regression algorithm with the given data to create and store a new residual model. " +
        "Does not activate the generated model automatically. " +
        "Returns the generated expression and the ID of the stored model."
    )]
    public Task<CallToolResult> TrainResidualModel(
        [Description("The start time (inclusive) for the data to use for training")]
        DateTimeOffset startTimeIncl,
        int populationSize,
        int maxIterations,
        CancellationToken ct = default
    ) => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Training residual model");

        var data = await dataStorage.GetLastDataAsync(startTimeIncl)
            .Select(d => new DataDto(d.Item1, d.Item2))
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

        var baseModel = await modelService.GetBaseModelAsync(ct);

        if (baseModel is not null)
        {
            var baseModelEvaluator = new SymbolicRegressionModel(baseModel, Interpreter);
            var dataset = Dataset.FromRowData(variableNames, datasetRows);
            var basePredictions = baseModelEvaluator.Predict(dataset, Enumerable.Range(0, dataset.Rows)).ToArray();

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
        }

        var request = new SymbolicRegressionRequestDto
        {
            Hyperparameters = new SymbolicRegressionHyperparametersDto
            {
                Base = new HyperparametersDto
                {
                    PopulationSize = populationSize,
                    MaxIterations = maxIterations,
                },
            },
            Dataset = new SymbolicRegressionDatasetDto
            {
                Data = datasetRows,
                VariableNames = variableNames,
                TargetVariableName = "y",
            },
        };

        var expression = await client.RunSymRegAsync(request, ct);
        
        var id = await modelStorage.SaveModelAsync(expression);

        responseStream.Broadcast(EventType.Tool, "Residual model training done");

        return TextResult(JsonSerializer.Serialize(
            new ModelDto
            {
                ModelId = id,
                Expression = expression,
            },
            JsonOptions
        ));
    });

    [UsedImplicitly, McpServerTool, Description(
         "Runs a symbolic regression algorithm with the given data to train and store the base model. " +
         "Returns the generated expression."
     )]
    public Task<CallToolResult> TrainBaseModelAsync(
        [Description("The start time (inclusive) for the data to use for training, or null to use all available data")]
        DateTimeOffset? startTimeIncl = null,
        CancellationToken ct = default
    ) => DoAsync(async () =>
    {
        startTimeIncl ??= DateTimeOffset.MinValue;

        responseStream.Broadcast(EventType.Tool, "Training base model");

        var data = await dataStorage.GetLastDataAsync(startTimeIncl.Value)
            .Select(d => new DataDto(d.Item1, d.Item2))
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

        var request = new SymbolicRegressionRequestDto
        {
            Dataset = new SymbolicRegressionDatasetDto
            {
                Data = datasetRows,
                VariableNames = variableNames,
                TargetVariableName = "y",
            },
        };

        var expression = await client.RunSymRegAsync(request, ct);
        
        await modelStorage.SaveBaseModelAsync(expression);
        modelService.SetActiveModel(0);

        responseStream.Broadcast(EventType.Tool, "Base model training done");

        return TextResult(JsonSerializer.Serialize(
            new
            {
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

        var data = await dataStorage.GetLastDataAsync(startTimeIncl)
            .Select(d => (Timestamp: d.Item1, Values: d.Item2))
            .ToArrayAsync(cancellationToken: ct);

        if (data.Length == 0)
        {
            throw new ArgumentException("No data available in the specified range.");
        }

        var combinedModel = await modelService.GetCombinedModelAsync(modelId, ct);

        if (combinedModel is null)
        {
            throw new InvalidOperationException("No model available to evaluate.");
        }

        var valuesOnly = data.Select(d => d.Values).ToArray();
        var qualityList = modelAnalyzer.EvaluateQualityOverTime(combinedModel, valuesOnly, windowSize);

        var qualityOverTime = data.Zip(qualityList, (d, q) => new
        {
            d.Timestamp,
            Quality = q,
        });

        return TextResult(JsonSerializer.Serialize(qualityOverTime, JsonOptions));
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets the base model (the original model without any residuals)")]
    public Task<CallToolResult> GetBaseModel(CancellationToken ct = default) => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Retrieving base model");
        
        var baseModel = await modelService.GetBaseModelAsync(ct);
        if (baseModel == null)
        {
            return TextResult("No base model has been trained yet.");
        }

        return TextResult(InfixExpressionFormatter.Format(baseModel, NumberFormatInfo.InvariantInfo));
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets the residual model by ID")]
    public Task<CallToolResult> GetResidualModel(
        [Description("The ID of the model to retrieve, or the active one if not provided")]
        int? modelId = null,
        CancellationToken ct = default
    ) => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Retrieving residual model");

        var residualModel = await modelService.GetResidualModelAsync(modelId, ct);
        return TextResult(InfixExpressionFormatter.Format(residualModel, NumberFormatInfo.InvariantInfo));
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets the combined model (base + residual) by ID")]
    public Task<CallToolResult> GetCombinedModel(
        [Description("The ID of the model to retrieve, or the active one if not provided")]
        int? modelId = null,
        CancellationToken ct = default
    ) => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Retrieving combined model");

        var combinedModel = await modelService.GetCombinedModelAsync(modelId, ct);

        if (combinedModel is null)
        {
            return TextResult("No model available.");
        }

        return TextResult(
            InfixExpressionFormatter.Format(combinedModel, NumberFormatInfo.InvariantInfo)
        );
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Gets all saved models in the model store")]
    public Task<CallToolResult> GetAllSavedModelsAsync() => DoAsync(async () =>
    {
        responseStream.Broadcast(EventType.Tool, "Retrieving all saved models");

        var models = await modelStorage.GetAllResidualModelsAsync().ToArrayAsync();

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

        var models = modelStorage.GetAllResidualModelsAsync();

        if (await models.AllAsync(m => m.Id != modelId))
        {
            throw new ArgumentException($"Model with ID {modelId} not found.");
        }

        modelService.SetActiveModel(modelId);

        return TextResult($"Active model set to ID {modelId}");
    });

    [UsedImplicitly]
    [McpServerTool]
    [Description("Enables or disables showing the permutation feature importances in the user's chart.")]
    public void SetShowPermutationFeatureImportances(bool show)
        => features.FeatureImportance = show;
}
