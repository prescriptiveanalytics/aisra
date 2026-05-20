using HEAL.HeuristicAgent.Web.Mcp.Client;
using HEAL.HeuristicAgent.Web.Services;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace HEAL.HeuristicAgent.Web.Chat;

public interface IHeuristicChatClient
{
    IAsyncEnumerable<string> GetStreamingResponseAsync(ICollection<ChatMessage> messages);
}

public sealed class HeuristicChatClient(
    IChatClient chatClient,
    McpClientProvider mcpClientProvider,
    ICancellationTokenProvider ctp,
    IConfiguration config
) : IHeuristicChatClient
{
    private readonly List<ChatMessage> _messageHistory =
    [
        new(
            ChatRole.System,
            File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "SKILL.md"))
        )
    ];

    private async Task<List<AITool>> GetToolsAsync()
    {
        var ct = ctp.Token;
        var tools = await (await mcpClientProvider.GetClientAsync()).ListToolsAsync(cancellationToken: ct);

        return tools.Cast<AITool>().ToList();
    }

    public async IAsyncEnumerable<string> GetStreamingResponseAsync(ICollection<ChatMessage> messages)
    {
        _messageHistory.AddRange(messages);
        var text = "";

        await foreach (
            var update in chatClient.GetStreamingResponseAsync(
                _messageHistory,
                new ChatOptions
                {
                    Tools = await GetToolsAsync(),
                },
                cancellationToken: ctp.Token
            )
        )
        {
            if (string.IsNullOrEmpty(update.Text))
            {
                continue;
            }

            yield return update.Text;
            text += update.Text;
        }

        _messageHistory.Add(new ChatMessage(ChatRole.Assistant, text));
    }
}
