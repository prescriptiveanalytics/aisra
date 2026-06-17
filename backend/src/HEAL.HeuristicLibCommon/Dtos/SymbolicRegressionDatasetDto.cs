using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace HEAL.HeuristicLibContracts.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct SymbolicRegressionDatasetDto
{
    [JsonRequired]
    public required string[] VariableNames { get; init; }

    [JsonRequired]
    public required double[][] Data { get; init; }

    [JsonRequired]
    public required string TargetVariableName { get; init; }
}
