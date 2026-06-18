using System.Globalization;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;
using AIsra.Common.Dtos;
using AIsra.Common.Random;
using AIsra.HeuristicLibAdapter.Lib.Runners;

namespace AIsra.HeuristicLibAdapter.Lib;

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
