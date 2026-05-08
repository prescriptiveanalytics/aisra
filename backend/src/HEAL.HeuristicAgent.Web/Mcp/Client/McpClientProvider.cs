using ModelContextProtocol.Client;

namespace HEAL.HeuristicAgent.Web.Mcp.Client;

public sealed class McpClientProvider(IClientTransport transport) : IAsyncDisposable
{
    private McpClient? _client;

    public async Task<McpClient> GetClientAsync()
        => _client ??= await McpClient.CreateAsync(transport);

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
        {
            await _client.DisposeAsync();
        }
    }
}
