using HEAL.HeuristicAgent.Shared.Dtos;
using HEAL.HeuristicGrpc.Core.Proto;
using Riok.Mapperly.Abstractions;

namespace HEAL.HeuristicWeb.Grpc.Core.Mapping;

[Mapper]
public static partial class FuncProblemMapper
{
    public static partial FuncProblemDto ToDto(this GrpcFuncProblem grpcFuncProblem);
    public static partial GrpcFuncProblem ToGrpc(this FuncProblemDto funcProblemDto);
}
