using Grpc.Core;
using HEAL.HeuristicGrpc.Core.Mapping;
using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicLibWrapper.Core.Runners;

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
