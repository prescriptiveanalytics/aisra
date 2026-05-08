using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record AllowedSymbolsDto
{
    public SymbolType[] Symbols { get; init; } = Enum.GetValues<SymbolType>();
    public string[] Variables { get; init; } = ["*"];
}
