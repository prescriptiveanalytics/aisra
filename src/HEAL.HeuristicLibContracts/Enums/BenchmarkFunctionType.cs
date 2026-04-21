using System.Text.Json.Serialization;

namespace HEAL.HeuristicLibContracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<BenchmarkFunctionType>))]
public enum BenchmarkFunctionType
{
    Ackley,
    Griewank,
    Rastrigin,
    Rosenbrock,
    Sphere,
}
