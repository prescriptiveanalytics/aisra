using System.Runtime.InteropServices;

namespace HEAL.HeuristicAgent.Web.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct ChatRequest
{
    public required string Message { get; init; }
}
