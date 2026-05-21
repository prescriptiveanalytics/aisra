using System.ClientModel;
using System.Diagnostics;
using System.IO.Pipelines;
using HEAL.HeuristicAgent.Web.Chat;
using HEAL.HeuristicAgent.Web.Mcp.Client;
using HEAL.HeuristicAgent.Web.Persistence;
using HEAL.HeuristicAgent.Web.Services;
using HEAL.HeuristicLibAdapter;
using HEAL.HeuristicLibAdapter.Grpc;
using HEAL.HeuristicLibWrapper;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI;

var (clientToServerPipe, serverToClientPipe) = MakeTwo<Pipe>();

var clientTransport = new StreamClientTransport(
    clientToServerPipe.Writer.AsStream(),
    serverToClientPipe.Reader.AsStream()
);

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5297, listenOptions =>
    {
        listenOptions.UseHttps();
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

services.AddControllers();

services.AddCors(options =>
{
    options.AddDefaultPolicy(policy
        => policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

var cfg = builder.Configuration;

if (!Enum.TryParse<ClientType>(cfg["ClientType"], true, out var clientType))
{
    throw new InvalidOperationException(
        "Invalid ClientType configuration. " +
        $"ClientType must be one of: {string.Join(", ", Enum.GetNames<ClientType>())}."
    );
}

var apiKey = cfg["OpenRouterApiKey"].NotBlankOrThrow(
    new InvalidOperationException(
        "OpenRouter API key not set. " +
        "Initialize the .NET Secret Manager with 'dotnet user-secrets init' " +
        "and set the API key with 'dotnet user-secrets set \"OpenRouterApiKey\" \"YOUR_API_KEY\"'."
    )
);

cfg["Model"] ??= "openrouter/free";
Console.WriteLine("Model: " + cfg["Model"]);

var openAiChatClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri("https://openrouter.ai/api/v1/"),
        NetworkTimeout = 60.Minutes,
    }
).GetChatClient(cfg["Model"]).AsIChatClient();

var chatClient = new ChatClientBuilder(openAiChatClient)
    .UseFunctionInvocation()
    .Build();

await using var cs = new CancellationService();

services.AddMcpServer()
    .WithStreamServerTransport(
        clientToServerPipe.Reader.AsStream(),
        serverToClientPipe.Writer.AsStream()
    )
    .WithToolsFromAssembly();
services
    .AddSingleton(chatClient)
    .AddSingleton<IClientTransport>(clientTransport)
    .AddSingleton<McpClientProvider>()
    .AddSingleton<IHeuristicChatClient, HeuristicChatClient>()
    .AddSingleton<IHeuristicLibClient>(
        clientType switch
        {
            ClientType.Grpc => new HeuristicLibGrpcClient(
                cfg["Server"]
                ?? throw new InvalidOperationException(
                    "Server URL must be provided in configuration."
                )
            ),
            ClientType.Lib => new HeuristicLibClient(),
            _ => throw new UnreachableException(),
        }
    )
    .AddSingleton<ICancellationTokenProvider>(cs)
    .AddSingleton<IDataClient, DataClient>()
    .AddSingleton<LlmResponseStream>()
    .AddSingleton<LlmClient>()
    .AddHostedService(sp => sp.GetRequiredService<LlmClient>())
    .AddSingleton<IModelStore>(new RedisModelStore(cfg["RedisHost"] ?? "localhost:6379"))
    .AddSingleton<IModelService, ModelService>()
    .AddSingleton<IModelQualityService, ModelQualityService>();

var app = builder.Build();

app.UseCors();

app.MapControllers();

await app.RunAsync();

return;

internal enum ClientType
{
    Lib,
    Grpc,
}
