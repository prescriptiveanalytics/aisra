using System.Text.Json.Serialization;

namespace HEAL.HeuristicRest.Services.Storage;

[JsonConverter(typeof(JsonStringEnumConverter<TrainingStatus>))]
public enum TrainingStatus
{
    Running,
    Successful,
    Failed,
}
