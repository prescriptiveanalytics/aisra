using System.Text.Json.Serialization;

namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record SymbolicRegressionRequestDto
{
    public SymbolicRegressionHyperparametersDto Hyperparameters { get; init; } = new();

    [JsonRequired]
    public required SymbolicRegressionDatasetDto Dataset { get; init; }

    public AllowedSymbolsDto AllowedSymbols { get; init; } = new();
}
