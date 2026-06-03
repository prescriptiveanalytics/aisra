using HEAL.HeuristicLib.Genotypes.Trees;

namespace HEAL.HeuristicAgent.Web.Services.Modeling;

public interface IModelService
{
    Task<SymbolicExpressionTree?> GetBaseModelAsync(CancellationToken ct);
    Task<SymbolicExpressionTree> GetResidualModelAsync(int? modelId, CancellationToken ct);
    Task<SymbolicExpressionTree?> GetCombinedModelAsync(int? modelId, CancellationToken ct);
    void SetActiveModel(int modelId);
}
