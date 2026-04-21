using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicWeb.Grpc.Core.Mapping;

public static class SymRegProblemMapper
{
    public static SymbolicRegressionHyperparametersDto ToDto(this GrpcSymbolicRegressionHyperparameters parameters)
        => new()
        {
            Base = new()
            {
                PopulationSize = parameters.Base.PopulationSize,
                MaxIterations = parameters.Base.MaxIterations,
            },
            Dataset = new()
            {
                Data = parameters.Dataset.Data.Select(row => row.Values.ToArray()).ToArray(),
                VariableNames = parameters.Dataset.VariableNames.ToArray(),
                TargetVariableName = parameters.Dataset.TargetVariableName,
            },
            AllowedSymbols = new()
            {
                Variables = parameters.AllowedSymbols.Variables.ToArray(),
                Symbols = parameters.AllowedSymbols.Symbols.Select(Enum.Parse<SymbolType>).ToArray(),
            },
            Mutators = parameters.Mutators.Select(Enum.Parse<Mutator>).ToArray(),
            SearchSpace = new()
            {
                TreeDepth = parameters.SearchSpace.TreeDepth,
                TreeLength = parameters.SearchSpace.TreeLength,
            },
            ParameterOptimizationIterations = parameters.ParameterOptimizationIterations,
        };

    public static GrpcSymbolicRegressionHyperparameters ToGrpc(this SymbolicRegressionHyperparametersDto dto)
    {
        var grpc = new GrpcSymbolicRegressionHyperparameters
        {
            Base = new()
            {
                PopulationSize = dto.Base.PopulationSize,
                MaxIterations = dto.Base.MaxIterations,
            },
            Dataset = new()
            {
                Data =
                {
                    dto.Dataset.Data.Select(row => new GrpcDoubleArray { Values = { row } })
                },
                VariableNames = { dto.Dataset.VariableNames },
                TargetVariableName = dto.Dataset.TargetVariableName,
            },
            AllowedSymbols = new()
            {
                Variables = { dto.AllowedSymbols.Variables },
                Symbols = { dto.AllowedSymbols.Symbols.Select(s => s.ToString()) }
            },
            Mutators = { dto.Mutators.Select(m => m.ToString()) },
            SearchSpace = new()
            {
                TreeDepth = dto.SearchSpace.TreeDepth,
                TreeLength = dto.SearchSpace.TreeLength,
            },
            ParameterOptimizationIterations = dto.ParameterOptimizationIterations,
        };

        return grpc;
    }
}
