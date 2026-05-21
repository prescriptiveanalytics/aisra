using System.Globalization;
using HEAL.HeuristicAgent.Web.Persistence;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;

namespace HEAL.HeuristicAgent.Web.Services;

public sealed class ModelService(IModelStore modelStore) : IModelService
{
    private static readonly InfixExpressionParser Parser = new();

    private static readonly SymbolicExpressionTree BaseModel =
        Parser.Parse("'x0' * 'x0' + 'x1' / 2 + 7");

    private int _activeModelId = 1;

    public SymbolicExpressionTree GetBaseModel() => BaseModel;

    public void SetActiveModel(int modelId) => _activeModelId = modelId;

    private async Task<SymbolicExpressionTree> GetResidualModelByIdAsync(int modelId)
    {
        var modelDto = await modelStore
            .GetAllModelsAsync()
            .FirstOrDefaultAsync(m => m.Id == modelId);
        var residualExpression = modelDto?.Model ?? "0";

        return Parser.Parse(residualExpression);
    }

    public async Task<SymbolicExpressionTree> GetResidualModelAsync(int? modelId = null)
    {
        var idToUse = modelId ?? _activeModelId;

        return await GetResidualModelByIdAsync(idToUse);
    }

    public async Task<SymbolicExpressionTree> GetCombinedModelAsync(int? modelId = null)
    {
        var residualModel = await GetResidualModelAsync(modelId);

        return Parser.Parse(
            $"({InfixExpressionFormatter.Format(BaseModel, NumberFormatInfo.InvariantInfo)})"
            + $" + ({InfixExpressionFormatter.Format(residualModel, NumberFormatInfo.InvariantInfo)})"
        );
    }
}
