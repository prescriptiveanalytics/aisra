using System.Globalization;
using Grpc.Core;
using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;
using HEAL.HeuristicLibWrapper.Runners;
using HEAL.HeuristicWeb.Grpc.Core.Mapping;

namespace HEAL.HeuristicWeb.Server.Grpc;

public sealed class HeuristicService : GrpcHeuristicService.GrpcHeuristicServiceBase
{
    public override async Task<GrpcBenchmarkSolution> RunBenchmark(GrpcFuncProblem request, ServerCallContext context)
    {
        try
        {
            var solution = new GrpcBenchmarkSolution();
            solution.Values.AddRange(await BenchmarkRunner.RunAsync(request.ToDto()));

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
                    await SymRegRunner.RunAsync(request.ToDto()),
                    NumberFormatInfo.InvariantInfo
                )
            };
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal,
                $"An error occurred while running symbolic regression: {ex.Message}"));
        }
    }
}
