namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record SymbolicRegressionSearchSpaceDto
{
    public int TreeDepth { get; init; } = 20;
    public int TreeLength { get; init; } = 50;
}
