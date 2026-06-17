using HEAL.HeuristicLib.Genotypes.Trees;

namespace AIsra.Web.Services.Modeling;

public interface IModelService
{
    int ActiveModelId { get; }
    event Action<int>? ActiveModelChanged;
    Task<SymbolicExpressionTree?> GetBaseModelAsync(CancellationToken ct);
    Task<SymbolicExpressionTree> GetResidualModelAsync(int? modelId, CancellationToken ct);
    Task<SymbolicExpressionTree?> GetCombinedModelAsync(int? modelId, CancellationToken ct);
    void SetActiveModel(int modelId);
}
