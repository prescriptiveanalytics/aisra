using System.Diagnostics;
using HEAL.HeuristicLibAdapter;
using HEAL.HeuristicLibAdapter.Grpc;
using HEAL.HeuristicLibWrapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    .AddJsonFile("appsettings.json");

builder.Logging.ClearProviders();
builder.Logging.AddConsole(opt =>
    opt.LogToStandardErrorThreshold = LogLevel.Trace
);

if (!Enum.TryParse<ClientType>(builder.Configuration["ClientType"], true, out var clientType))
{
    throw new InvalidOperationException(
        "Invalid ClientType configuration. " +
        $"ClientType must be one of: {string.Join(", ", Enum.GetNames<ClientType>())}."
    );
}

var serverUrl = builder.Configuration["Server"];

builder.Services
    .AddSingleton<IHeuristicLibClient>(
        clientType switch
        {
            ClientType.Grpc => new HeuristicLibGrpcClient(
                serverUrl ?? throw new InvalidOperationException("Server URL must be provided in configuration.")
            ),
            ClientType.Lib => new HeuristicLibClient(),
            _ => throw new UnreachableException(),
        }
    );
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

internal enum ClientType
{
    Lib,
    Grpc,
}
