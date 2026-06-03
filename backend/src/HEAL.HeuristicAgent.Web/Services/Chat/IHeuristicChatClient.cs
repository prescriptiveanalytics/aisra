using Microsoft.Extensions.AI;

namespace HEAL.HeuristicAgent.Web.Services.Chat;

public interface IHeuristicChatClient
{
    IAsyncEnumerable<string> GetStreamingResponseAsync(ICollection<ChatMessage> messages);
}
