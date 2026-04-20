using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record SymRegProblemDto
{
    public ProblemDto Problem { get; init; } = new();

    public required string[] VariableNames { get; init; }
    public required double[][] Data { get; init; }

    public Mutator[] Mutators { get; init; } =
    [
        Mutator.ChangeNodeTypeManipulation,
        Mutator.FullTreeShaker,
        Mutator.OnePointShaker,
        Mutator.RemoveBranchManipulation,
        Mutator.ReplaceBranchManipulation,
    ];
}
