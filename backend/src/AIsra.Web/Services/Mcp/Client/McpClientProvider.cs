using ModelContextProtocol.Client;

namespace AIsra.Web.Services.Mcp.Client;

/// <summary>
/// Provides a method for obtaining an MCP client.
/// This is necessary, because the MCP client will try to connect to the MCP server as soon as it is created,
/// so it has to be created when the server is already running.
/// </summary>
/// <param name="transport">The transport mechanism to use for communication with the MCP server.</param>
public sealed class McpClientProvider(IClientTransport transport) : IAsyncDisposable
{
    private McpClient? client;

    /// <summary>
    /// Lazily creates and returns an MCP client.
    /// </summary>
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
