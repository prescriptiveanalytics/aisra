using System.Runtime.InteropServices;

namespace AIsra.Common.Dtos;

[StructLayout(LayoutKind.Auto)]
public readonly record struct SymbolicRegressionSearchSpaceDto(int TreeDepth = 20, int TreeLength = 50)
{
    public SymbolicRegressionSearchSpaceDto() : this(20)
    {
    }
}
