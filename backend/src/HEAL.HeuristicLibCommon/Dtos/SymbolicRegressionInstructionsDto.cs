using System.Text.Json.Serialization;

namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record SymbolicRegressionInstructionsDto
{
    public SymbolicRegressionHyperparametersDto Hyperparameters { get; init; } = new();

    [JsonRequired]
    public required DateTimeOffset StartTimeIncl { get; init; }
    public DateTimeOffset? EndTimeExcl { get; init; }
}
