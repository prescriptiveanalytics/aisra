using Microsoft.Extensions.AI;

namespace AIsra.Web.Services.Chat;

public interface IHeuristicChatClient
{
    IAsyncEnumerable<string> GetStreamingResponseAsync(ICollection<ChatMessage> messages);
}
