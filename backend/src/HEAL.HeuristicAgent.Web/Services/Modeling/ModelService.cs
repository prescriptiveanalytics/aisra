using HEAL.HeuristicAgent.Web.Services.Persistence;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;

namespace HEAL.HeuristicAgent.Web.Services.Modeling;

public sealed class ModelService(IModelStorage modelStorage) : IModelService
{
    private static readonly InfixExpressionParser Parser = new();

    private int activeModelId = 1;

    public int ActiveModelId => activeModelId;

    public event Action<int>? ActiveModelChanged;

    public async Task<SymbolicExpressionTree?> GetBaseModelAsync(CancellationToken ct)
    {
        var modelString = await modelStorage.GetBaseModelAsync();
        return modelString == null ? null : Parser.Parse(modelString);
    }

    public void SetActiveModel(int modelId)
    {
        activeModelId = modelId;
        ActiveModelChanged?.Invoke(modelId);
    }

    public async Task<SymbolicExpressionTree?> GetCombinedModelAsync(int? modelId, CancellationToken ct)
    {
        var idToUse = modelId ?? activeModelId;

        var baseModelTask = modelStorage.GetBaseModelAsync();
        var residualModelTask = modelStorage.GetResidualModelAsync(idToUse);

        await Task.WhenAll(baseModelTask, residualModelTask);

        var baseString = baseModelTask.Result;
        var residualString = residualModelTask.Result ?? "0";

        if (baseString is null)
        {
            return null;
        }

        return Parser.Parse($"({baseString}) + ({residualString})");
    }

    public async Task<SymbolicExpressionTree> GetResidualModelAsync(int? modelId, CancellationToken ct)
    {
        var idToUse = modelId ?? activeModelId;
        var residualString = await modelStorage.GetResidualModelAsync(idToUse);

        return Parser.Parse(residualString ?? "0");
    }
}
