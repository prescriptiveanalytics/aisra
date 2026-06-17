using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AIsra.Web.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct ModelDto
{
    [UsedImplicitly]
    public required int ModelId { get; init; }

    [UsedImplicitly]
    public required string Expression { get; init; }
}
