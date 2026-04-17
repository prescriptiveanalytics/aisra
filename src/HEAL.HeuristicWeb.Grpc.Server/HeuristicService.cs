using Grpc.Core;
using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicWeb.Grpc.Core.Mapping;
using HEAL.HeuristicLibWrapper.Runners;

namespace HEAL.HeuristicGrpc.Server;

public sealed class HeuristicService : GrpcHeuristicService.GrpcHeuristicServiceBase
{
    public override async Task<GrpcSolution> RunBenchmark(GrpcFuncProblem request, ServerCallContext context)
    {
        var solution = new GrpcSolution();
        solution.Values.AddRange(await BenchmarkRunner.RunAsync(request.ToDto()));

        return solution;
    }
}
