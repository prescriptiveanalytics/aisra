using ModelContextProtocol.Client;

namespace HEAL.HeuristicAgent.Web.Services.Mcp.Client;

public sealed class McpClientProvider(IClientTransport transport) : IAsyncDisposable
{
    private McpClient? client;

    public async Task<McpClient> GetClientAsync()
        => client ??= await McpClient.CreateAsync(transport);

    public async ValueTask DisposeAsync()
    {
        if (client is not null)
        {
            await client.DisposeAsync();
        }
    }
}
