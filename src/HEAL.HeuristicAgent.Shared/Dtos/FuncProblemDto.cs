namespace HEAL.HeuristicAgent.Shared.Dtos;

public record FuncProblemDto
{
    public required string Function { get; init; }
    public int Dimensions { get; init; } = 2;
    public int Population { get; init; } = 100;
    public double MutationRate { get; init; } = 0.5;
    public double MutationStrength { get; init; } = 0.5;
    public int MaxIterations { get; init; } = 10_000;
}
