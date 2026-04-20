using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicWeb.Grpc.Core.Mapping;

public static class SymRegProblemMapper
{
    public static SymRegProblemDto ToDto(this GrpcSymRegProblem problem)
        => new()
        {
            Problem = new()
            {
                PopulationSize = problem.Problem.PopulationSize,
                MaxIterations = problem.Problem.MaxIterations,
            },
            VariableNames = problem.VariableNames.ToArray(),
            Data = problem.Data.Select(row => row.Values.ToArray()).ToArray(),
            Mutators = problem.Mutators.Select(m => Enum.Parse<Mutator>(m, true)).ToArray(),
        };

    public static GrpcSymRegProblem ToGrpc(this SymRegProblemDto dto)
    {
        var grpc = new GrpcSymRegProblem
        {
            Problem = new()
            {
                PopulationSize = dto.Problem.PopulationSize,
                MaxIterations = dto.Problem.MaxIterations,
            },
        };

        grpc.VariableNames.AddRange(dto.VariableNames);
        grpc.Data.AddRange(dto.Data.Select(row => new DoubleArray { Values = { row } }));
        grpc.Mutators.AddRange(dto.Mutators.Select(m => m.ToString()));

        return grpc;
    }
}
