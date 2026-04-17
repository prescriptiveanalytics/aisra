using System.Text.Json.Serialization;

namespace HEAL.HeuristicWeb.Rest.Server.Services.Storage;

[JsonConverter(typeof(JsonStringEnumConverter<TrainingStatus>))]
public enum TrainingStatus
{
    Running,
    Successful,
    Failed,
}
