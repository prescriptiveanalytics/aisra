using System.Globalization;
using HEAL.HeuristicAgent.Web.Persistence;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;

namespace HEAL.HeuristicAgent.Web.Services;

public sealed class ModelService(IModelStore modelStore) : IModelService
{
    internal const string BaseModelString = "'x1' * 'x1' + 'x2' * 'x2' / 2 + 7";

    private static readonly InfixExpressionParser Parser = new();

    private static readonly SymbolicExpressionTree BaseModel =
        Parser.Parse(BaseModelString);

    private int _activeModelId = 1;

    public SymbolicExpressionTree GetBaseModel() => BaseModel;

    public void SetActiveModel(int modelId) => _activeModelId = modelId;

    public async Task<SymbolicExpressionTree> GetCombinedModelAsync(int? modelId, CancellationToken ct)
    {
        var residualModel = await GetResidualModelAsync(modelId, ct);

        return Parser.Parse(
            $"({InfixExpressionFormatter.Format(BaseModel, NumberFormatInfo.InvariantInfo)})"
            + $" + ({InfixExpressionFormatter.Format(residualModel, NumberFormatInfo.InvariantInfo)})"
        );
    }

    public async Task<SymbolicExpressionTree> GetResidualModelAsync(int? modelId, CancellationToken ct)
    {
        var idToUse = modelId ?? _activeModelId;

        return await GetResidualModelByIdAsync(idToUse, ct);
    }

    private async Task<SymbolicExpressionTree> GetResidualModelByIdAsync(int modelId, CancellationToken ct)
    {
        var modelDto = await modelStore
            .GetAllModelsAsync()
            .FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken: ct);
        var residualExpression = modelDto?.Model ?? "0";

        return Parser.Parse(residualExpression);
    }
}
