using HEAL.HeuristicLibContracts.Dtos;

namespace HEAL.HeuristicLibAdapter;

public interface IHeuristicLibClient : IDisposable
{
    Task<double[]> RunBenchmarkAsync(BenchmarkHyperparametersDto dto, CancellationToken ct = default);
    Task<string> RunSymRegAsync(SymbolicRegressionHyperparametersDto dto, CancellationToken ct = default);
}
