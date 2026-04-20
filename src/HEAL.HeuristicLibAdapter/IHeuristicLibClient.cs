using HEAL.HeuristicLibContracts.Dtos;

namespace HEAL.HeuristicLibAdapter;

public interface IHeuristicLibClient : IDisposable
{
    Task<double[]> RunBenchmarkAsync(FuncProblemDto dto, CancellationToken ct = default);
    Task<string> RunSymRegAsync(SymRegProblemDto dto, CancellationToken ct = default);
}
