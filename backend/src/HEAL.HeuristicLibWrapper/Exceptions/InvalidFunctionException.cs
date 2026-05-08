using HEAL.HeuristicLibContracts.Enums;

namespace HEAL.HeuristicLibWrapper.Exceptions;

public sealed class InvalidFunctionException() : Exception(
    $"Function name is invalid. Supported functions are: {
        string.Join(", ", Enum.GetNames<BenchmarkFunctionType>().Select(t => $"'{t}'"))
    }."
);
