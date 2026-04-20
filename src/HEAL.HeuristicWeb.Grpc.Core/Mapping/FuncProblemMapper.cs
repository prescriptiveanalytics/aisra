using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicGrpc.Core.Proto;

namespace HEAL.HeuristicWeb.Grpc.Core.Mapping;

public static class FuncProblemMapper
{
    public static FuncProblemDto ToDto(this GrpcFuncProblem grpc)
        => new()
        {
            Problem = new()
            {
                PopulationSize = grpc.Problem.PopulationSize,
                MaxIterations = grpc.Problem.MaxIterations,
            },
            Function = grpc.Function,
            Dimensions = grpc.Dimensions,
            MutationRate = grpc.MutationRate,
            MutationStrength = grpc.MutationStrength,
        };

    public static GrpcFuncProblem ToGrpc(this FuncProblemDto dto)
        => new()
        {
            Problem = new()
            {
                PopulationSize = dto.Problem.PopulationSize,
                MaxIterations = dto.Problem.MaxIterations,
            },
            Function = dto.Function,
            Dimensions = dto.Dimensions,
            MutationRate = dto.MutationRate,
            MutationStrength = dto.MutationStrength,
        };
}
