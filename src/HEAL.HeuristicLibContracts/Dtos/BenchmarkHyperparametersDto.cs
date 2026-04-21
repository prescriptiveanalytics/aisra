using System.Text.Json.Serialization;
using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicLibContracts.Dtos;

public record BenchmarkHyperparametersDto
{
    [JsonRequired]
    public required HyperparametersDto Problem { get; init; }

    [JsonRequired]
    public required BenchmarkFunctionType Function { get; init; }

    public int Dimensions { get; init; } = 2;
    public double MutationRate { get; init; } = 0.5;
    public double MutationStrength { get; init; } = 0.5;
}
