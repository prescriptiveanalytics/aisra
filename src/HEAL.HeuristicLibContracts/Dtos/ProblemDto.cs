namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record ProblemDto
{
    public int PopulationSize { get; init; } = 100;
    public int MaxIterations { get; init; } = 10_000;
}
