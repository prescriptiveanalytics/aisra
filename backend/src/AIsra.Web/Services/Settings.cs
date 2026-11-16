using AIsra.Web.Services.Reset;

namespace AIsra.Web.Services;

public sealed class Settings : IResettable
{
    private const bool DefaultFeatureImportance = false;
    private const bool DefaultMonitorModelQuality = true;

    public bool FeatureImportance { get; set; } = DefaultFeatureImportance;

    /// <summary>
    /// Whether the agent is prompted when the model quality falls below the threshold.
    /// </summary>
    public bool MonitorModelQuality { get; set; } = DefaultMonitorModelQuality;

    public Task ResetAsync(CancellationToken ct)
    {
        FeatureImportance = DefaultFeatureImportance;
        MonitorModelQuality = DefaultMonitorModelQuality;

        return Task.CompletedTask;
    }
}
