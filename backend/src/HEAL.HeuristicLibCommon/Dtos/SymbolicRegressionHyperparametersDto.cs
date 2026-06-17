using System.Runtime.InteropServices;
using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicLibContracts.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct SymbolicRegressionHyperparametersDto()
{
    public HyperparametersDto Base { get; init; } = new();

    public SymbolicRegressionSearchSpaceDto SearchSpace { get; init; } = new();

    public Mutator[] Mutators { get; init; } =
    [
        Mutator.ChangeNodeTypeManipulation,
        Mutator.FullTreeShaker,
        Mutator.OnePointShaker,
        Mutator.RemoveBranchManipulation,
        Mutator.ReplaceBranchManipulation,
    ];

    public int ParameterOptimizationIterations { get; init; } = 5;
}
