using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AIsra.Web.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct ModelMetricsDto()
{
    [UsedImplicitly]
    public required double Quality { get; init; }

    [UsedImplicitly]
    public FeatureImportanceDto[]? FeatureImportances { get; init; } = null;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FeatureImportanceDto
{
    [UsedImplicitly]
    public required string Feature { get; init; }

    [UsedImplicitly]
    public required double Importance { get; init; }
}
