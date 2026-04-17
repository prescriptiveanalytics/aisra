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

    public static TestFunction? FromString(string funcName, int dimensions)
        => FromType(GetTypeFromString(funcName), dimensions);

    private static TestFunction? FromType(TestFunctionType? type, int dimensions)
    {
        IGradientTestFunction? func = type switch
        {
            TestFunctionType.Ackley => new AckleyFunction(dimensions),
            TestFunctionType.Griewank => new GriewankFunction(dimensions),
            TestFunctionType.Rastrigin => new RastriginFunction(dimensions),
            TestFunctionType.Rosenbrock => new RosenbrockFunction(dimensions),
            TestFunctionType.Sphere => new SphereFunction(dimensions),
            _ => null,
        };

        return func is null ? null : new(func);
    }

    private static TestFunctionType? GetTypeFromString(string typeName)
        => Enum.TryParse<TestFunctionType>(typeName, out var type) ? type : null;

    public double Evaluate(RealVector solution) => _function.Evaluate(solution);

    public int Dimension => _function.Dimension;
    public double Min => _function.Min;
    public double Max => _function.Max;
    public ObjectiveDirection Objective => _function.Objective;
    public RealVector EvaluateGradient(RealVector solution) => _function.EvaluateGradient(solution);
}
