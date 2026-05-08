using System.Globalization;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;
using HEAL.HeuristicLibAdapter;
using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicLibWrapper.Runners;

namespace HEAL.HeuristicLibWrapper;

public sealed class HeuristicLibClient : IHeuristicLibClient
{
    public Task<double[]> RunBenchmarkAsync(BenchmarkHyperparametersDto dto, CancellationToken ct = default)
        => BenchmarkRunner.RunAsync(dto, ct);

    public async Task<string> RunSymRegAsync(SymbolicRegressionRequestDto dto, CancellationToken ct = default)
        => InfixExpressionFormatter.Format(await SymRegRunner.RunAsync(dto, ct), NumberFormatInfo.InvariantInfo);

    public void Dispose()
    {
    }
}
