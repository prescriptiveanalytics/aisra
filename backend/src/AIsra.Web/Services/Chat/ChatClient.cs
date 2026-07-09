using System.Reflection;
using AIsra.Web.Services.Mcp.Client;
using AIsra.Common.Random;
using AIsra.Common.Threading;
using Microsoft.Extensions.AI;

namespace AIsra.Web.Services.Chat;

public sealed class ChatClient(
    IConfiguration config,
    IChatClient chatClient,
    McpClientProvider mcpClientProvider,
    IRng rng,
    ICancellationTokenProvider ctp
) : IHeuristicChatClient
{
    private static readonly string SystemPrompt = Assembly.GetExecutingAssembly()
        .Let(asm => asm.ReadEmbeddedTextFile($"{asm.GetName().Name}.Resources.Prompt.md"));

    private readonly int maxHistoryChars = int.TryParse(config["MAX_HISTORY_CHARS"], out var n) ? n : 20_000;
    private readonly double temperature = double.TryParse(config["LLM_TEMPERATURE"], out var temp) ? temp : 0.7;
    private readonly List<ChatMessage> messageHistory = [new(ChatRole.System, SystemPrompt)];
    private int totalHistoryChars = SystemPrompt.Length;
    private List<AITool>? cachedTools;

    private async Task<List<AITool>> GetToolsAsync()
    {
        if (cachedTools is not null)
        {
            return cachedTools;
        }

        var ct = ctp.Token;
        var tools = await (await mcpClientProvider.GetClientAsync()).ListToolsAsync(cancellationToken: ct);

        return cachedTools = tools.Cast<AITool>().ToList();
    }

    public async IAsyncEnumerable<string> GetStreamingResponseAsync(ICollection<ChatMessage> messages)
    {
        foreach (var msg in messages)
        {
            messageHistory.Add(msg);
            totalHistoryChars += msg.Contents.OfType<TextContent>().FirstOrDefault()?.Text.Length ?? 0;
        }

        TrimHistory();

        var text = "";

        await foreach (
            var update in chatClient.GetStreamingResponseAsync(
                messageHistory,
                new ChatOptions
                {
                    Tools = await GetToolsAsync(),
                    Seed = rng.Next(),
                    Temperature = (float)temperature,
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
        totalHistoryChars += text.Length;
        TrimHistory();
    }

    private void TrimHistory()
    {
        while (totalHistoryChars > maxHistoryChars && messageHistory.Count > 1)
        {
            var removed = messageHistory[1];
            var removedLength = removed.Contents.OfType<TextContent>().FirstOrDefault()?.Text.Length ?? 0;
            totalHistoryChars -= removedLength;
            messageHistory.RemoveAt(1);
        }
    }
}
