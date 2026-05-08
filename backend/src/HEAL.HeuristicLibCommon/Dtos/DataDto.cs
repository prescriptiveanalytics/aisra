namespace HEAL.HeuristicLibContracts.Dtos;

public sealed record DataDto
{
    public required DateTimeOffset Timestamp { get; init; }
    public required double[] Data { get; init; }
}
