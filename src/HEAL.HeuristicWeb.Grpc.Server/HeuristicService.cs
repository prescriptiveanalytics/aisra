using System.Globalization;
using Grpc.Core;
using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicLib.Problems.DataAnalysis.Formatter;
using HEAL.HeuristicWeb.Grpc.Core.Mapping;
using HEAL.HeuristicLibWrapper.Runners;

namespace HEAL.HeuristicGrpc.Server;

public sealed class HeuristicService : GrpcHeuristicService.GrpcHeuristicServiceBase
{
    public override async Task<GrpcBenchmarkSolution> RunBenchmark(GrpcFuncProblem request, ServerCallContext context)
    {
        var solution = new GrpcBenchmarkSolution();
        solution.Values.AddRange(await BenchmarkRunner.RunAsync(request.ToDto()));

        return solution;
    }

    public override async Task<GrpcSymRegSolution> Fit(GrpcSymRegProblem request, ServerCallContext context)
    {
        return new()
        {
            Expression = InfixExpressionFormatter.Format(await SymRegRunner.RunAsync(request.ToDto()),
                NumberFormatInfo.InvariantInfo)
        };
    }
}
