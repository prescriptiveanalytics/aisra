using System.Diagnostics;
using HEAL.HeuristicLib.Algorithms;
using HEAL.HeuristicLib.Algorithms.Evolutionary;
using HEAL.HeuristicLib.Genotypes.Vectors;
using HEAL.HeuristicLib.Operators.Creators.RealVectorCreators;
using HEAL.HeuristicLib.Operators.Crossovers.RealVectorCrossovers;
using HEAL.HeuristicLib.Operators.Mutators.RealVectorMutators;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems;
using HEAL.HeuristicLib.Random;
using HEAL.HeuristicLib.SearchSpaces.Vectors;
using HEAL.HeuristicLibWrapper.Dtos;
using HEAL.HeuristicLibWrapper.Exceptions;
using HEAL.HeuristicLibWrapper.Heuristic;

namespace HEAL.HeuristicLibWrapper.Runners;

public static class BenchmarkRunner
{
    private static RealVectorSearchSpace GetSpace(TestFunction func) => new(func.Dimension, func.Min, func.Max);

    private static FuncProblem<RealVector, RealVectorSearchSpace> GetProblem(TestFunction func)
        => new(func.Evaluate, GetSpace(func), func.Objective.ToObjective());

    private static GeneticAlgorithm<RealVector, RealVectorSearchSpace, FuncProblem<RealVector, RealVectorSearchSpace>>
        BuildAlgorithm(TestFunction func, FuncProblemDto dto)
        => new GeneticAlgorithmBuilder<RealVector, RealVectorSearchSpace,
            FuncProblem<RealVector, RealVectorSearchSpace>>
        {
            Creator = new UniformDistributedCreator(func.Min, func.Max),
            Crossover = new AlphaBetaBlendCrossover(),
            Mutator = new GaussianMutator(dto.MutationRate, dto.MutationStrength),
            PopulationSize = dto.Population,
        }.Build();

    public static async Task<double[]> RunAsync(FuncProblemDto dto)
    {
        var func = TestFunction.FromString(dto.Function, dto.Dimensions);

        if (func is null)
        {
            throw new InvalidFunctionException(dto.Function);
        }

        var alg = BuildAlgorithm(func, dto);
        var problem = GetProblem(func);

        var i = 0;
        ISolution<RealVector>? best = null;
        await foreach (var state in alg.RunStreamingAsync(problem, RandomNumberGenerator.Create(Random.Shared.Next())))
        {
            best = state.Population.Solutions
                .MinBy(s => s.ObjectiveVector, problem.Objective.TotalOrderComparer);

            if (++i > dto.MaxIterations)
            {
                break;
            }
        }

        if (best is null)
        {
            throw new HeuristicAlgorithmException("Algorithm did not produce any solutions.");
        }

        return best.Genotype.ToArray();
    }

    private static Objective ToObjective(this ObjectiveDirection direction) =>
        direction switch
        {
            ObjectiveDirection.Minimize => SingleObjective.Minimize,
            ObjectiveDirection.Maximize => SingleObjective.Maximize,
            _ => throw new UnreachableException(),
        };
}
