using System.Runtime.InteropServices;

namespace AIsra.Web.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct AgentSettingsDto
{
    public required bool MonitorModelQuality { get; init; }
}
