using System.Diagnostics.CodeAnalysis;
using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicWeb.Server.Rest.Storage;

public sealed class TypedSolutionStorage<TGenotype>(SolutionStorage storage) where TGenotype : notnull
{
    public void Store(
        Guid id, TGenotype? solution = default,
        TrainingStatus status = TrainingStatus.Running
    ) => storage.Store(id, solution, status);

    public bool TryGet(
        Guid id,
        [NotNullWhen(true)] out TGenotype? solution,
        [NotNullWhen(true)] out TrainingStatus? status
    ) => storage.TryGet(id, out solution, out status);
}
