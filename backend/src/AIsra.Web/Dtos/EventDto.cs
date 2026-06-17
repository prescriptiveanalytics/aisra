using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AIsra.Web.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct EventDto
{
    [UsedImplicitly]
    public required string Message { get; init; }
}
