using System.Diagnostics.CodeAnalysis;

namespace HEAL.HeuristicRest.Services.Storage;

public sealed class TypedSolutionStore<TGenotype>(SolutionStore store) where TGenotype : notnull
{
    public void Store(
        Guid id, TGenotype? solution = default,
        TrainingStatus status = TrainingStatus.Running
    ) => store.Store(id, solution, status);

    public bool TryGet(
        Guid id,
        [NotNullWhen(true)] out TGenotype? solution,
        [NotNullWhen(true)] out TrainingStatus? status
    ) => store.TryGet(id, out solution, out status);
}
