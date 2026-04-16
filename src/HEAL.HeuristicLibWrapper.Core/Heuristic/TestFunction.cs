using HEAL.HeuristicLib.Genotypes.Vectors;
using HEAL.HeuristicLib.Optimization;
using HEAL.HeuristicLib.Problems.TestFunctions;
using HEAL.HeuristicLib.Problems.TestFunctions.SingleObjectives;

namespace HEAL.HeuristicLibWrapper.Core.Heuristic;

public sealed class TestFunction : IGradientTestFunction
{
    private readonly IGradientTestFunction _function;

    private TestFunction(IGradientTestFunction function)
    {
        _function = function;
    }

    public static TestFunction? FromString(string funcName, int dimensions)
        => FromType(GetTypeFromString(funcName), dimensions);

    private static TestFunction? FromType(Type? type, int dimensions)
    {
        IGradientTestFunction? func = type switch
        {
            Type.Ackley => new AckleyFunction(dimensions),
            Type.Griewank => new GriewankFunction(dimensions),
            Type.Rastrigin => new RastriginFunction(dimensions),
            Type.Rosenbrock => new RosenbrockFunction(dimensions),
            Type.Sphere => new SphereFunction(dimensions),
            _ => null,
        };

        return func is null ? null : new(func);
    }

    private static Type? GetTypeFromString(string typeName)
        => typeName.ToLower() switch
        {
            "ackley" => Type.Ackley,
            "griewank" => Type.Griewank,
            "rastrigin" => Type.Rastrigin,
            "rosenbrock" => Type.Rosenbrock,
            "sphere" => Type.Sphere,
            _ => null,
        };

    public double Evaluate(RealVector solution) => _function.Evaluate(solution);

    public int Dimension => _function.Dimension;
    public double Min => _function.Min;
    public double Max => _function.Max;
    public ObjectiveDirection Objective => _function.Objective;
    public RealVector EvaluateGradient(RealVector solution) => _function.EvaluateGradient(solution);

    public enum Type
    {
        Ackley,
        Griewank,
        Rastrigin,
        Rosenbrock,
        Sphere,
    }
}
