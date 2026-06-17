using System.Diagnostics;
using AIsra.Common.Dtos;
using AIsra.HeuristicLibWrapper.Exceptions;
using AIsra.HeuristicLibWrapper.Heuristic;
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

namespace AIsra.HeuristicLibWrapper.Runners;

public static class BenchmarkRunner
{
    private static RealVectorSearchSpace GetSpace(TestFunction func) => new(func.Dimension, func.Min, func.Max);

    private static FuncProblem<RealVector, RealVectorSearchSpace> GetProblem(TestFunction func)
        => new(func.Evaluate, GetSpace(func), func.Objective.ToObjective());

    private static TerminatableAlgorithm<RealVector, RealVectorSearchSpace,
            FuncProblem<RealVector, RealVectorSearchSpace>, PopulationState<RealVector>>
        BuildAlgorithm(TestFunction func, BenchmarkHyperparametersDto dto)
        => new()
        {
            Algorithm = new GeneticAlgorithmBuilder<RealVector, RealVectorSearchSpace,
                FuncProblem<RealVector, RealVectorSearchSpace>>
            {
                Creator = new UniformDistributedCreator(func.Min, func.Max),
                Crossover = new AlphaBetaBlendCrossover(),
                Mutator = new GaussianMutator(dto.MutationRate, dto.MutationStrength),
                PopulationSize = dto.Problem.PopulationSize,
            }.Build(),
            Terminator = new AfterIterationsTerminator<RealVector>(dto.Problem.MaxIterations),
        };

    public static async Task<double[]> RunAsync(BenchmarkHyperparametersDto dto, int? seed, CancellationToken ct)
    {
        var func = TestFunction.FromType(dto.Function, dto.Dimensions);

        if (func is null)
        {
            throw new InvalidFunctionException();
        }

        var alg = BuildAlgorithm(func, dto);
        var problem = GetProblem(func);
        var result = await alg.RunToCompletionAsync(problem, RandomNumberGenerator.Create(seed ?? Random.Shared.Next()), ct: ct);

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
