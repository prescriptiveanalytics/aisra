using HEAL.HeuristicLib.Genotypes.Trees;

namespace HEAL.HeuristicAgent.Web.Services;

public interface IModelService
{
    Task<SymbolicExpressionTree?> GetBaseModelAsync(CancellationToken ct = default);
    Task<SymbolicExpressionTree> GetResidualModelAsync(int? modelId, CancellationToken ct);
    Task<SymbolicExpressionTree?> GetCombinedModelAsync(int? modelId, CancellationToken ct);
    void SetActiveModel(int modelId);
}
