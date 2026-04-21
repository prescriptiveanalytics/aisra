using HEAL.HeuristicLib.Genotypes.Vectors;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems.TestFunctions;
using HEAL.HeuristicLib.Problems.TestFunctions.SingleObjectives;
using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicLibWrapper.Heuristic;

public sealed class TestFunction : IGradientTestFunction
{
    private readonly IGradientTestFunction _function;

    private TestFunction(IGradientTestFunction function)
    {
        _function = function;
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

        return func is null ? null : new(func);
    }

    public double Evaluate(RealVector solution) => _function.Evaluate(solution);

    public int Dimension => _function.Dimension;
    public double Min => _function.Min;
    public double Max => _function.Max;
    public ObjectiveDirection Objective => _function.Objective;
    public RealVector EvaluateGradient(RealVector solution) => _function.EvaluateGradient(solution);
}
