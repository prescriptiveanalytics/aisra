using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicLibWrapper.Dtos;
using Riok.Mapperly.Abstractions;

namespace HEAL.HeuristicGrpc.Core.Mapping;

[Mapper]
public static partial class FuncProblemMapper
{
    public static partial FuncProblemDto ToDto(this GrpcFuncProblem grpcFuncProblem);
    public static partial GrpcFuncProblem ToGrpc(this FuncProblemDto funcProblemDto);
}
