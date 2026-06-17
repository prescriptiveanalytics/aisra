using System.Text.Json.Serialization;

namespace AIsra.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<Mutator>))]
public enum Mutator
{
    ChangeNodeTypeManipulation,
    FullTreeShaker,
    OnePointShaker,
    RemoveBranchManipulation,
    ReplaceBranchManipulation,
}
