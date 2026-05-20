using HEAL.HeuristicLib.Genotypes.Trees;

namespace HEAL.HeuristicAgent.Web.Services;

public interface IModelService
{
    SymbolicExpressionTree GetBaseModel();
    Task<SymbolicExpressionTree> GetResidualModelAsync(int? modelId = null);
    Task<SymbolicExpressionTree> GetCombinedModelAsync(int? modelId = null);
    void SetActiveModel(int modelId);
}
