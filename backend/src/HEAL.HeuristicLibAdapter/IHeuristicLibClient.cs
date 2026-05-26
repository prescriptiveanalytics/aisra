using HEAL.HeuristicLibContracts.Dtos;

namespace HEAL.HeuristicLibAdapter;

public interface IHeuristicLibClient : IDisposable
{
    Task<double[]> RunBenchmarkAsync(BenchmarkHyperparametersDto dto, CancellationToken ct);
    Task<string> RunSymRegAsync(SymbolicRegressionRequestDto dto, CancellationToken ct);
}
