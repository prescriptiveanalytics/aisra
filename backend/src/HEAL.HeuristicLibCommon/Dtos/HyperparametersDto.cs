namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record HyperparametersDto
{
    public int PopulationSize { get; init; } = 100;
    public int MaxIterations { get; init; } = 100;
}
