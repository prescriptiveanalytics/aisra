using System.Text.Json.Serialization;
using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record SymbolicRegressionHyperparametersDto
{
    public HyperparametersDto Base { get; init; } = new();

    [JsonRequired]
    public required SymbolicRegressionDatasetDto Dataset { get; init; }

    public SymbolicRegressionSearchSpaceDto SearchSpace { get; init; } = new();

    public Mutator[] Mutators { get; init; } =
    [
        Mutator.ChangeNodeTypeManipulation,
        Mutator.FullTreeShaker,
        Mutator.OnePointShaker,
        Mutator.RemoveBranchManipulation,
        Mutator.ReplaceBranchManipulation,
    ];

    public AllowedSymbolsDto AllowedSymbols { get; init; } = new();

    public int ParameterOptimizationIterations { get; init; } = 5;
}
