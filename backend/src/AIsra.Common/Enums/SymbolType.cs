using System.Text.Json.Serialization;

namespace AIsra.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<SymbolType>))]
public enum SymbolType
{
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Number,
    SquareRoot,
    Logarithm,
    Exponential,
}
