namespace HEAL.HeuristicLibContracts.Dtos;

public record FuncProblemDto
{
    public required ProblemDto Problem { get; init; }

    public required string Function { get; init; }
    public int Dimensions { get; init; } = 2;
    public double MutationRate { get; init; } = 0.5;
    public double MutationStrength { get; init; } = 0.5;
}
