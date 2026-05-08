using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicWeb.Grpc.Core.Mapping;

public static class FuncProblemMapper
{
    public static BenchmarkHyperparametersDto ToDto(this GrpcFuncProblem grpc)
        => new()
        {
            Problem = new HyperparametersDto
            {
                PopulationSize = grpc.Problem.PopulationSize,
                MaxIterations = grpc.Problem.MaxIterations,
            },
            Function = Enum.Parse<BenchmarkFunctionType>(grpc.Function),
            Dimensions = grpc.Dimensions,
            MutationRate = grpc.MutationRate,
            MutationStrength = grpc.MutationStrength,
        };

    public static GrpcFuncProblem ToGrpc(this BenchmarkHyperparametersDto dto)
        => new()
        {
            Problem = new GrpcHyperparameters
            {
                PopulationSize = dto.Problem.PopulationSize,
                MaxIterations = dto.Problem.MaxIterations,
            },
            Function = dto.Function.ToString(),
            Dimensions = dto.Dimensions,
            MutationRate = dto.MutationRate,
            MutationStrength = dto.MutationStrength,
        };
}
