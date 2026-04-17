using System.Text.Json.Serialization;

namespace HEAL.HeuristicLibContracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<TrainingStatus>))]
public enum TrainingStatus
{
    Running,
    Successful,
    Failed,
}
