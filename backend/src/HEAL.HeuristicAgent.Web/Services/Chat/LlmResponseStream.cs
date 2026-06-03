using HEAL.HeuristicAgent.Web.Dtos;

namespace HEAL.HeuristicAgent.Web.Services.Chat;

public sealed class LlmResponseStream
{
    public event Action<EventType, string?>? OnMessage;

    public void Broadcast(EventType eventType, string? message = null)
        => OnMessage?.Invoke(eventType, message);
}
