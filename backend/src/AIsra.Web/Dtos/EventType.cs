namespace AIsra.Web.Dtos;

/// <summary>
/// Defines the types of events that can be broadcast to the frontend.
/// </summary>
public enum EventType
{
    /// <summary>
    /// The agent has started responding and can be interrupted.
    /// Also sent when a client connects while the agent is busy.
    /// </summary>
    Started,

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
    /// Also sent when a client connects while the agent is idle.
    /// </summary>
    Done,

    /// <summary>
    /// The LLM response was interrupted by the user.
    /// </summary>
    Interrupted,

    /// <summary>
    /// The application was reset, so all state shown in the frontend is outdated.
    /// </summary>
    Reset,
}
