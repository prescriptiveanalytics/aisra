using System.Collections.Immutable;
using HEAL.HeuristicLib.Algorithms;
using HEAL.HeuristicLib.Algorithms.Evolutionary;
using HEAL.HeuristicLib.Algorithms.MetaAlgorithms;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Operators;
using HEAL.HeuristicLib.Operators.Creators.SymbolicExpressionTreeCreators;
using HEAL.HeuristicLib.Operators.Crossovers.SymbolicExpressionTreeCrossovers;
using HEAL.HeuristicLib.Operators.Mutators;
using HEAL.HeuristicLib.Operators.Mutators.SymbolicExpressionTreeMutators;
using HEAL.HeuristicLib.Operators.Terminators;
using HEAL.HeuristicLib.Problems.DataAnalysis;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression;
using HEAL.HeuristicLib.Problems.DataAnalysis.Regression.Evaluators;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces.Trees;
using HEAL.HeuristicLib.SearchSpaces.Trees.SymbolicExpressionTree.Grammars;
using HEAL.HeuristicLib.SearchSpaces.Trees.SymbolicExpressionTree.Symbols;
using HEAL.HeuristicLib.SearchSpaces.Trees.SymbolicExpressionTree.Symbols.Math;
using HEAL.HeuristicLib.States;
using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicLibContracts.Enums;
using HEAL.HeuristicLibWrapper.Exceptions;

namespace HEAL.HeuristicLibWrapper.Runners;

public static class SymRegRunner
{
    public static async Task<SymbolicExpressionTree> RunAsync(
        SymbolicRegressionRequestDto dto,
        int? seed,
        CancellationToken ct
    )
    {
        ImmutableArray<IMutator<SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace, SymbolicRegressionProblem>>
            mutators =
            [
                ..dto.Hyperparameters.Mutators.Select(m =>
                    (IMutator<SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace, SymbolicRegressionProblem>)(m
                        switch
                        {
                            Mutator.ChangeNodeTypeManipulation => new ChangeNodeTypeManipulation(),
                            Mutator.FullTreeShaker => new FullTreeShaker(),
                            Mutator.OnePointShaker => new OnePointShaker(),
                            Mutator.RemoveBranchManipulation => new RemoveBranchManipulation(),
                            Mutator.ReplaceBranchManipulation => new ReplaceBranchManipulation(),
                            _ => throw new ArgumentOutOfRangeException(nameof(m), m, null)
                        }))
            ];

        var alg =
            new TerminatableAlgorithm<SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace,
                SymbolicRegressionProblem, PopulationState<SymbolicExpressionTree>>
            {
                Algorithm = new GeneticAlgorithmBuilder<
                    SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace, SymbolicRegressionProblem
                >
                {
                    Creator = new ProbabilisticTreeCreator(),
                    Crossover = new SubtreeCrossover(),
                    Mutator = new PipelineMutator<
                        SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace, SymbolicRegressionProblem
                    >(mutators),
                    PopulationSize = dto.Hyperparameters.Base.PopulationSize,
                }.Build(),
                Terminator =
                    new AfterIterationsTerminator<SymbolicExpressionTree>(dto.Hyperparameters.Base.MaxIterations),
            };

        var dataset = Dataset.FromRowData(
            dto.Dataset.VariableNames,
            dto.Dataset.Data
        );
        var problem = new SymbolicRegressionProblem(
            new RegressionProblemData(dataset, dto.Dataset.TargetVariableName),
            new PearsonR2Evaluator()
        )
        {
            LowerPredictionBound = double.MinValue,
            UpperPredictionBound = double.MaxValue,
            SearchSpace =
            {
                TreeDepth = dto.Hyperparameters.SearchSpace.TreeDepth,
                TreeLength = dto.Hyperparameters.SearchSpace.TreeLength,
            },
            ParameterOptimizationIterations = dto.Hyperparameters.ParameterOptimizationIterations,
            DegreeOfParallelism = Environment.ProcessorCount * 3 / 4,
        };

        var linearScalingRoot = problem.SearchSpace.Grammar.AddLinearScaling();

        var symbols = dto.AllowedSymbols.Symbols.Select(s => (Symbol)(s switch
        {
            SymbolType.Addition => new Addition(),
            SymbolType.Subtraction => new Subtraction(),
            SymbolType.Multiplication => new Multiplication(),
            SymbolType.Division => new Division(),
            SymbolType.Number => new Number(),
            SymbolType.SquareRoot => new SquareRoot(),
            SymbolType.Logarithm => new Logarithm(),
            SymbolType.Exponential => new Exponential(),
            _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
        })).Append(new Variable
        {
            VariableNames = dto.AllowedSymbols.Variables.Any(s => s.Trim() == "*")
                ? dto.Dataset.VariableNames.Except([dto.Dataset.TargetVariableName]).ToArray()
                : dto.AllowedSymbols.Variables
        }).ToArray();

        problem.SearchSpace.Grammar.AddFullyConnectedSymbols(linearScalingRoot, symbols);

        var state = await alg.RunToCompletionAsync(problem, RandomNumberGenerator.Create(seed ?? Random.Shared.Next()), ct: ct);

        return state.Population.Solutions
                   .MinBy(s => s.ObjectiveVector, problem.Objective.TotalOrderComparer)?.Genotype
               ?? throw new HeuristicAlgorithmException("Algorithm did not produce any solutions.");
    }
}
