namespace AIsra.Web.Services;

public sealed class Settings
{
    public bool FeatureImportance { get; set; }

    /// <summary>
    /// Whether the agent is prompted when the model quality falls below the threshold.
    /// </summary>
    public bool MonitorModelQuality { get; set; } = true;
}
