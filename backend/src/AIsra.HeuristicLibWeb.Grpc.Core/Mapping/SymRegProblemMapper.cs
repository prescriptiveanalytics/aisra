using AIsra.HeuristicLibWeb.Grpc.Core.Proto;
using AIsra.Common.Dtos;
using AIsra.Common.Enums;

namespace AIsra.HeuristicLibWeb.Grpc.Core.Mapping;

public static class SymRegProblemMapper
{
    public static SymbolicRegressionRequestDto ToDto(this GrpcSymbolicRegressionHyperparameters parameters)
        => new()
        {
            Hyperparameters = new SymbolicRegressionHyperparametersDto
            {
                Base = new HyperparametersDto
                {
                    PopulationSize = parameters.Base.PopulationSize,
                    MaxIterations = parameters.Base.MaxIterations,
                },
                Mutators = parameters.Mutators.Select(Enum.Parse<Mutator>).ToArray(),
                SearchSpace = new SymbolicRegressionSearchSpaceDto
                {
                    TreeDepth = parameters.SearchSpace.TreeDepth,
                    TreeLength = parameters.SearchSpace.TreeLength,
                },
                ParameterOptimizationIterations = parameters.ParameterOptimizationIterations,
            },
            AllowedSymbols = new AllowedSymbolsDto
            {
                Variables = parameters.AllowedSymbols.Variables.ToArray(),
                Symbols = parameters.AllowedSymbols.Symbols.Select(Enum.Parse<SymbolType>).ToArray(),
            },
            Dataset = new SymbolicRegressionDatasetDto
            {
                Data = parameters.Dataset.Data.Select(row => row.Values.ToArray()).ToArray(),
                VariableNames = parameters.Dataset.VariableNames.ToArray(),
                TargetVariableName = parameters.Dataset.TargetVariableName,
            },
        };

    public static GrpcSymbolicRegressionHyperparameters ToGrpc(this SymbolicRegressionRequestDto dto)
    {
        var grpc = new GrpcSymbolicRegressionHyperparameters
        {
            Base = new GrpcHyperparameters
            {
                PopulationSize = dto.Hyperparameters.Base.PopulationSize,
                MaxIterations = dto.Hyperparameters.Base.MaxIterations,
            },
            Dataset = new GrpcSymbolicRegressionDataset
            {
                Data =
                {
                    dto.Dataset.Data.Select(row => new GrpcDoubleArray { Values = { row } }),
                },
                VariableNames = { dto.Dataset.VariableNames },
                TargetVariableName = dto.Dataset.TargetVariableName,
            },
            AllowedSymbols = new GrpcAllowedSymbols
            {
                Variables = { dto.AllowedSymbols.Variables },
                Symbols = { dto.AllowedSymbols.Symbols.Select(s => s.ToString()) },
            },
            Mutators = { dto.Hyperparameters.Mutators.Select(m => m.ToString()) },
            SearchSpace = new GrpcSymbolicRegressionSearchSpace
            {
                TreeDepth = dto.Hyperparameters.SearchSpace.TreeDepth,
                TreeLength = dto.Hyperparameters.SearchSpace.TreeLength,
            },
            ParameterOptimizationIterations = dto.Hyperparameters.ParameterOptimizationIterations,
        };

        return grpc;
    }
}
