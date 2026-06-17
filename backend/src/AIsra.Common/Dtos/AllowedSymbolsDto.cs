using System.Runtime.InteropServices;
using AIsra.Common.Enums;

namespace AIsra.Common.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct AllowedSymbolsDto()
{
    public SymbolType[] Symbols { get; init; } = Enum.GetValues<SymbolType>();
    public string[] Variables { get; init; } = ["*"];
}
