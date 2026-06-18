using HEAL.HeuristicLib.Genotypes.Vectors;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems.TestFunctions;
using HEAL.HeuristicLib.Problems.TestFunctions.SingleObjectives;
using AIsra.Common.Enums;

namespace AIsra.HeuristicLibAdapter.Lib.Heuristic;

public sealed class TestFunction : IGradientTestFunction
{
    private readonly IGradientTestFunction function;

    private TestFunction(IGradientTestFunction function)
    {
        this.function = function;
    }

    public static TestFunction? FromType(BenchmarkFunctionType? type, int dimensions)
    {
        IGradientTestFunction? func = type switch
        {
            BenchmarkFunctionType.Ackley => new AckleyFunction(dimensions),
            BenchmarkFunctionType.Griewank => new GriewankFunction(dimensions),
            BenchmarkFunctionType.Rastrigin => new RastriginFunction(dimensions),
            BenchmarkFunctionType.Rosenbrock => new RosenbrockFunction(dimensions),
            BenchmarkFunctionType.Sphere => new SphereFunction(dimensions),
            _ => null,
        };

        return func is null ? null : new TestFunction(func);
    }

    public double Evaluate(RealVector solution) => function.Evaluate(solution);

    public int Dimension => function.Dimension;
    public double Min => function.Min;
    public double Max => function.Max;
    public ObjectiveDirection Objective => function.Objective;
    public RealVector EvaluateGradient(RealVector solution) => function.EvaluateGradient(solution);
}
