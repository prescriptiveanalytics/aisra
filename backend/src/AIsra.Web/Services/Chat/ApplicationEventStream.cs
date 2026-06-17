using AIsra.Web.Dtos;

namespace AIsra.Web.Services.Chat;

public sealed class ApplicationEventStream
{
    public event Action<EventType, string?>? OnMessage;

    public void Broadcast(EventType eventType, string? message = null)
        => OnMessage?.Invoke(eventType, message);
}
