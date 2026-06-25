using System.Globalization;
using Grpc.Core;
using AIsra.HeuristicLibWeb.Grpc.Core.Proto;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;
using AIsra.Common.Random;
using AIsra.Common.Threading;
using AIsra.HeuristicLibAdapter.Lib.Runners;
using AIsra.HeuristicLibWeb.Grpc.Core.Mapping;

namespace AIsra.HeuristicLibWeb.Server.Grpc;

public sealed class HeuristicService(IRng rng, ICancellationTokenProvider ctp) : GrpcHeuristicService.GrpcHeuristicServiceBase
{
    public override async Task<GrpcBenchmarkSolution> RunBenchmark(GrpcFuncProblem request, ServerCallContext context)
    {
        try
        {
            var solution = new GrpcBenchmarkSolution();
            solution.Values.AddRange(await BenchmarkRunner.RunAsync(request.ToDto(), rng.Next(), ctp.Token));

            return solution;
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal,
                $"An error occurred while running the benchmark: {ex.Message}"));
        }
    }

    public override async Task<GrpcSymbolicRegressionSolution> Fit(
        GrpcSymbolicRegressionHyperparameters request,
        ServerCallContext context
    )
    {
        try
        {
            return new GrpcSymbolicRegressionSolution
            {
                Expression = InfixExpressionFormatter.Format(
                    await SymRegRunner.RunAsync(request.ToDto(), rng.Next(), ctp.Token),
                    NumberFormatInfo.InvariantInfo
                ),
            };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal,
                $"An error occurred while running symbolic regression: {ex.Message}"));
        }
    }
}
