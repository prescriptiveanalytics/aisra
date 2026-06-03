using System.Globalization;
using HEAL.HeuristicAgent.Web.Services.Persistence;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;

namespace HEAL.HeuristicAgent.Web.Services.Modeling;

public sealed class ModelService(IModelStore modelStore) : IModelService
{
    private static readonly InfixExpressionParser Parser = new();

    private int activeModelId = 1;

    public async Task<SymbolicExpressionTree?> GetBaseModelAsync(CancellationToken ct = default)
    {
        var modelString = await modelStore.GetBaseModelAsync();
        return modelString == null ? null : Parser.Parse(modelString);
    }

    public void SetActiveModel(int modelId) => activeModelId = modelId;

    public async Task<SymbolicExpressionTree?> GetCombinedModelAsync(int? modelId, CancellationToken ct)
    {
        var residualModel = await GetResidualModelAsync(modelId, ct);
        var baseModel = await GetBaseModelAsync(ct);

        if (baseModel is null)
        {
            return null;
        }

        return Parser.Parse(
            $"({InfixExpressionFormatter.Format(baseModel, NumberFormatInfo.InvariantInfo)})"
            + $" + ({InfixExpressionFormatter.Format(residualModel, NumberFormatInfo.InvariantInfo)})"
        );
    }

    public async Task<SymbolicExpressionTree> GetResidualModelAsync(int? modelId, CancellationToken ct)
    {
        var idToUse = modelId ?? activeModelId;

        return await GetResidualModelByIdAsync(idToUse, ct);
    }

    private async Task<SymbolicExpressionTree> GetResidualModelByIdAsync(int modelId, CancellationToken ct)
    {
        var modelDto = await modelStore
            .GetAllResidualModelsAsync()
            .FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken: ct);
        var residualExpression = modelDto?.Model ?? "0";

        return Parser.Parse(residualExpression);
    }
}
