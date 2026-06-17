using System.Text.Json.Serialization;

namespace AIsra.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<TrainingStatus>))]
public enum TrainingStatus
{
    Running,
    Successful,
    Failed,
}
