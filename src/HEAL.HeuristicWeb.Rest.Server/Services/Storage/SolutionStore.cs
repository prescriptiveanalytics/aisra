using System.Diagnostics.CodeAnalysis;
using HEAL.HeuristicLibContracts.Enums;
using HEAL.HeuristicWeb.Rest.Server.Data;

namespace HEAL.HeuristicWeb.Rest.Server.Services.Storage;

public sealed class SolutionStore
{
    private readonly ConcurrentMultiTypeDictionary<Guid> _solutions = new();

    public void Store<TGenotype>(
        Guid id,
        TGenotype? solution = default,
        TrainingStatus? status = null
    ) where TGenotype : notnull
    {
        if (!_solutions.TryGet<StoredResult<TGenotype>>(id, out var result))
        {
            _solutions.Set(
                id,
                new StoredResult<TGenotype>
                {
                    Solution = solution,
                    Status = status ?? TrainingStatus.Running
                }
            );

            return;
        }

        if (solution is not null)
        {
            result.Solution = solution;
        }

        if (status is not null)
        {
            result.Status = status.Value;
        }
    }

    public bool TryGet<TGenotype>(
        Guid id,
        out TGenotype? solution,
        [NotNullWhen(true)] out TrainingStatus? status
    ) where TGenotype : notnull
    {
        status = null;
        solution = default;

        if (!_solutions.TryGet(id, out StoredResult<TGenotype>? result))
        {
            return false;
        }

        solution = result.Solution;
        status = result.Status;

        return true;
    }

    public TypedSolutionStore<TGenotype> ToTyped<TGenotype>() where TGenotype : notnull => new(this);

    public sealed record StoredResult<TGenotype>
    {
        public required TGenotype? Solution
        {
            get;
            set
            {
                if (value is null && Status == TrainingStatus.Successful)
                {
                    throw new InvalidOperationException("Cannot set solution to null if status is successful.");
                }

                field = value;
            }
        }

        public TrainingStatus Status
        {
            get;
            set
            {
                if (value == TrainingStatus.Successful && Solution is null)
                {
                    throw new InvalidOperationException("Cannot set status to successful if solution is null.");
                }

                field = value;
            }
        } = TrainingStatus.Running;
    }
}
