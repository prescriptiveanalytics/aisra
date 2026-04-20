using System.Diagnostics;
using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicLib.Algorithms;
using HEAL.HeuristicLib.Algorithms.Evolutionary;
using HEAL.HeuristicLib.Algorithms.MetaAlgorithms;
using HEAL.HeuristicLib.Genotypes.Vectors;
using HEAL.HeuristicLib.Operators.Creators.RealVectorCreators;
using HEAL.HeuristicLib.Operators.Crossovers.RealVectorCrossovers;
using HEAL.HeuristicLib.Operators.Mutators.RealVectorMutators;
using HEAL.HeuristicLib.Operators.Terminators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces.Vectors;
using HEAL.HeuristicLib.States;
using HEAL.HeuristicLibWrapper.Exceptions;
using HEAL.HeuristicLibWrapper.Heuristic;

namespace HEAL.HeuristicLibWrapper.Runners;

public static class BenchmarkRunner
{
    private static RealVectorSearchSpace GetSpace(TestFunction func) => new(func.Dimension, func.Min, func.Max);

    private static FuncProblem<RealVector, RealVectorSearchSpace> GetProblem(TestFunction func)
        => new(func.Evaluate, GetSpace(func), func.Objective.ToObjective());

    private static TerminatableAlgorithm<RealVector, RealVectorSearchSpace,
            FuncProblem<RealVector, RealVectorSearchSpace>, PopulationState<RealVector>>
        BuildAlgorithm(TestFunction func, FuncProblemDto dto)
        => new()
        {
            Algorithm = new GeneticAlgorithmBuilder<RealVector, RealVectorSearchSpace,
                FuncProblem<RealVector, RealVectorSearchSpace>>
            {
                Creator = new UniformDistributedCreator(func.Min, func.Max),
                Crossover = new AlphaBetaBlendCrossover(),
                Mutator = new GaussianMutator(dto.MutationRate, dto.MutationStrength),
                PopulationSize = dto.Population,
            }.Build(),
            Terminator = new AfterIterationsTerminator<RealVector>(dto.MaxIterations),
        };

    public static async Task<double[]> RunAsync(FuncProblemDto dto)
    {
        var func = TestFunction.FromString(dto.Function, dto.Dimensions);

        if (func is null)
        {
            throw new InvalidFunctionException(dto.Function);
        }

        var alg = BuildAlgorithm(func, dto);
        var problem = GetProblem(func);
        var result = await alg.RunToCompletionAsync(problem, RandomNumberGenerator.Create(Random.Shared.Next()));

        return result.Population.Solutions
                   .MinBy(s => s.ObjectiveVector, problem.Objective.TotalOrderComparer)?.Genotype.ToArray()
               ?? throw new HeuristicAlgorithmException("Algorithm did not produce any solutions.");
    }

    private static Objective ToObjective(this ObjectiveDirection direction) =>
        direction switch
        {
            ObjectiveDirection.Minimize => SingleObjective.Minimize,
            ObjectiveDirection.Maximize => SingleObjective.Maximize,
            _ => throw new UnreachableException(),
        };
}
