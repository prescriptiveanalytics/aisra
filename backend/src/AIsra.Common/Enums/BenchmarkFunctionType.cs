using System.Text.Json.Serialization;

namespace AIsra.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<BenchmarkFunctionType>))]
public enum BenchmarkFunctionType
{
    Ackley,
    Griewank,
    Rastrigin,
    Rosenbrock,
    Sphere,
}
