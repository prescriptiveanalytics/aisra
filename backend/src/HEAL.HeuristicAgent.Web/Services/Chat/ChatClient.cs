using System.Reflection;
using HEAL.HeuristicAgent.Web.Services.Mcp.Client;
using HEAL.HeuristicLibContracts.Random;
using HEAL.HeuristicLibContracts.Threading;
using Microsoft.Extensions.AI;

namespace HEAL.HeuristicAgent.Web.Services.Chat;

public sealed class ChatClient(
    IChatClient chatClient,
    McpClientProvider mcpClientProvider,
    IRng rng,
    ICancellationTokenProvider ctp
) : IHeuristicChatClient
{
    private readonly List<ChatMessage> messageHistory =
    [
        new(
            ChatRole.System,
            Assembly.GetExecutingAssembly().Let(asm
                => asm.ReadEmbeddedTextFile($"{asm.GetName().Name}.Resources.Prompt.md")
            )
        ),
    ];

    private async Task<List<AITool>> GetToolsAsync()
    {
        var ct = ctp.Token;
        var tools = await (await mcpClientProvider.GetClientAsync()).ListToolsAsync(cancellationToken: ct);

        return tools.Cast<AITool>().ToList();
    }

    public async IAsyncEnumerable<string> GetStreamingResponseAsync(ICollection<ChatMessage> messages)
    {
        messageHistory.AddRange(messages);
        var text = "";

        await foreach (
            var update in chatClient.GetStreamingResponseAsync(
                messageHistory,
                new ChatOptions
                {
                    Tools = await GetToolsAsync(),
                    Seed = rng.Next(),
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

        messageHistory.Add(new ChatMessage(ChatRole.Assistant, text));
    }
}
