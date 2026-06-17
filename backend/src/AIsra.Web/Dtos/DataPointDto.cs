using System.Runtime.InteropServices;

namespace AIsra.Web.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct DataPointDto
{
    public required string Id { get; init; }
    public required float Value { get; init; }
}
