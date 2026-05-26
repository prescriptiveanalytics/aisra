using System.Globalization;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;
using HEAL.HeuristicLibAdapter;
using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicLibContracts.Random;
using HEAL.HeuristicLibWrapper.Runners;

namespace HEAL.HeuristicLibWrapper;

public sealed class HeuristicLibClient(IRng rng) : IHeuristicLibClient
{
    public Task<double[]> RunBenchmarkAsync(BenchmarkHyperparametersDto dto, CancellationToken ct)
        => BenchmarkRunner.RunAsync(dto, rng.Next(), ct);

    public async Task<string> RunSymRegAsync(SymbolicRegressionRequestDto dto, CancellationToken ct)
        => InfixExpressionFormatter.Format(await SymRegRunner.RunAsync(dto, rng.Next(), ct), NumberFormatInfo.InvariantInfo);

    public void Dispose()
    {
    }
}
