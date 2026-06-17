using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AIsra.Web.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct SymbolicRegressionModelDto
{
    public required int Id { get; init; }

    [UsedImplicitly]
    public required string Model { get; init; }
}
