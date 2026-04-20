namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record SymRegSearchSpace
{
    public int TreeDepth { get; init; } = 20;
    public int TreeLength { get; init; } = 50;
}
