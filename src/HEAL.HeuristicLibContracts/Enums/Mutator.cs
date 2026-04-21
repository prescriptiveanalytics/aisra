using System.Text.Json.Serialization;

namespace HEAL.HeuristicLibContracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<Mutator>))]
public enum Mutator
{
    ChangeNodeTypeManipulation,
    FullTreeShaker,
    OnePointShaker,
    RemoveBranchManipulation,
    ReplaceBranchManipulation,
}
