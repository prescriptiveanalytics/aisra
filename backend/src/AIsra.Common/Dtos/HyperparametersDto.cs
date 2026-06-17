using System.Runtime.InteropServices;

namespace AIsra.Common.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct HyperparametersDto()
{
    public int PopulationSize { get; init; } = 100;
    public int MaxIterations { get; init; } = 100;
}
