using HEAL.HeuristicLib.Algorithms;
using HEAL.HeuristicLib.Algorithms.Evolutionary;
using HEAL.HeuristicLib.Algorithms.MetaAlgorithms;
using HEAL.HeuristicLib.Genotypes.Trees;
using HEAL.HeuristicLib.Operators.Creators.SymbolicExpressionTreeCreators;
using HEAL.HeuristicLib.Operators.Crossovers.SymbolicExpressionTreeCrossovers;
using HEAL.HeuristicLib.Operators.Mutators;
using HEAL.HeuristicLib.Operators.Mutators.SymbolicExpressionTreeMutators;
using HEAL.HeuristicLib.Operators.Terminators;
using HEAL.HeuristicLib.Optimization;
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

namespace HEAL.HeuristicLibWrapper.Runners;

public static class SymRegRunner
{
    public static async Task<SymbolicExpressionTree> RunAsync(SymRegProblemDto dto, CancellationToken ct = default)
    {
        var alg =
            new TerminatableAlgorithm<SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace,
                SymbolicRegressionProblem, PopulationState<SymbolicExpressionTree>>
            {
                Algorithm = new GeneticAlgorithmBuilder<SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace,
                    SymbolicRegressionProblem>
                {
                    Creator = new ProbabilisticTreeCreator(),
                    Crossover = new SubtreeCrossover(),
                    Mutator = new MultiMutator<SymbolicExpressionTree, SymbolicExpressionTreeSearchSpace,
                        SymbolicRegressionProblem>(
                        [
                            new ChangeNodeTypeManipulation(),
                            new FullTreeShaker(),
                            new OnePointShaker(),
                            new RemoveBranchManipulation(),
                            new ReplaceBranchManipulation()
                        ]
                    ),
                    PopulationSize = dto.Problem.PopulationSize,
                }.Build(),
                Terminator = new AfterIterationsTerminator<SymbolicExpressionTree>(dto.Problem.MaxIterations),
            };

        var dataset = Dataset.FromRowData(
            dto.VariableNames,
            dto.Data
        );
        var data = new RegressionProblemData(dataset);
        var problem = new SymbolicRegressionProblem(data, new PearsonR2Evaluator())
        {
            LowerPredictionBound = double.MinValue,
            UpperPredictionBound = double.MaxValue,
            SearchSpace =
            {
                TreeDepth = 20,
                TreeLength = 50
            },
            ParameterOptimizationIterations = 5
        };

        var linearScalingRoot = problem.SearchSpace.Grammar.AddLinearScaling();
        var symbols = new Symbol[]
        {
            new Addition(),
            new Subtraction(),
            new Multiplication(),
            new Division(),
            new Number(),
            new SquareRoot(),
            new Logarithm(),
            new Exponential(),
            new Variable { VariableNames = data.InputVariables }
        };

        problem.SearchSpace.Grammar.AddFullyConnectedSymbols(linearScalingRoot, symbols);

        var i = 0;
        ISolution<SymbolicExpressionTree>? best = null;
        await foreach (var state in alg.RunStreamingAsync(problem, RandomNumberGenerator.Create(Random.Shared.Next()), ct: ct))
        {
            best = state.Population.Solutions
                .MinBy(s => s.ObjectiveVector, problem.Objective.TotalOrderComparer);

            if (++i > 1000) break;
        }

        return best?.Genotype!;
    }
}
