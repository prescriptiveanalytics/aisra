using HEAL.HeuristicLibWrapper.Core.Heuristic;

namespace HEAL.HeuristicLibWrapper.Core.Exceptions;

public sealed class InvalidFunctionException(string functionName) : Exception(
    $"Function name '{functionName}' is invalid. Supported functions are: {
        string.Join(", ", Enum.GetNames<TestFunction.Type>().Select(t => $"'{t}'"))
    }."
);
