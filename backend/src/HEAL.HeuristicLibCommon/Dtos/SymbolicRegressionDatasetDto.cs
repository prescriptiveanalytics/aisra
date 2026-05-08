using System.Text.Json.Serialization;

namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record SymbolicRegressionDatasetDto
{
    [JsonRequired]
    public required string[] VariableNames { get; init; }

    [JsonRequired]
    public required double[][] Data { get; init; }

    [JsonRequired]
    public required string TargetVariableName { get; init; }
}
