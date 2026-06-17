namespace AIsra.Web.Dtos;

/// <summary>
/// Defines the types of events that can be broadcast to the frontend.
/// </summary>
public enum EventType
{
    /// <summary>
    /// A tool has been called.
    /// </summary>
    Tool,

    /// <summary>
    /// A new response fragment has been received from the LLM.
    /// </summary>
    Fragment,

    /// <summary>
    /// The model quality has fallen below the threshold.
    /// </summary>
    QualityDrop,

    /// <summary>
    /// The LLM is done responding.
    /// </summary>
    Done,
}
