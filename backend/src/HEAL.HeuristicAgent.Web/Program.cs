using System.ClientModel;
using System.Diagnostics;
using System.IO.Pipelines;
using HEAL.HeuristicAgent.Web.Services;
using HEAL.HeuristicAgent.Web.Services.Chat;
using HEAL.HeuristicAgent.Web.Services.Data;
using HEAL.HeuristicAgent.Web.Services.Mcp.Client;
using HEAL.HeuristicAgent.Web.Services.Modeling;
using HEAL.HeuristicAgent.Web.Services.Persistence;
using HEAL.HeuristicLibAdapter;
using HEAL.HeuristicLibAdapter.Grpc;
using HEAL.HeuristicLibContracts.Random;
using HEAL.HeuristicLibContracts.Threading;
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
var cfg = builder.Configuration;

var services = builder.Services;

builder.WebHost.ConfigureKestrel(options =>
{
    var httpPort = int.TryParse(cfg["HTTP_PORT"], out var parsedPort) ? parsedPort : 5297;
    var enableHttps = bool.TryParse(cfg["ENABLE_HTTPS"], out var parsedHttps) && parsedHttps;

    options.ListenAnyIP(httpPort, listenOptions =>
    {
        if (enableHttps)
        {
            listenOptions.UseHttps();
        }

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

if (string.IsNullOrWhiteSpace(cfg["Model"]))
{
    throw new InvalidOperationException("Model not specified in configuration.");
}

if (string.IsNullOrWhiteSpace(cfg["ModelEndpoint"]))
{
    throw new InvalidOperationException("ModelEndpoint not specified in configuration.");
}

var openAiChatClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(cfg["ModelEndpoint"]!),
    }
).GetChatClient(cfg["Model"]).AsIChatClient();

var chatClient = new ChatClientBuilder(openAiChatClient)
    .UseFunctionInvocation()
    .Build();

await using var cs = new CancellationService();
await using var redisStore = new RedisStorage(cfg["RedisHost"] ?? "localhost:6379");
var rng = new Rng();

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
    .AddSingleton<IHeuristicChatClient, ChatClient>()
    .AddSingleton<IHeuristicLibClient>(
        clientType switch
        {
            ClientType.Grpc => new HeuristicLibGrpcClient(
                cfg["Server"]
                ?? throw new InvalidOperationException(
                    "Server URL must be provided in configuration."
                )
            ),
            ClientType.Lib => new HeuristicLibClient(rng),
            _ => throw new UnreachableException(),
        }
    )
    .AddSingleton<ICancellationTokenProvider>(cs)
    .AddSingleton<IRng>(rng)
    .AddSingleton<IDataClient, MqttDataClient>()
    .AddSingleton<IDataAggregator, IntervalDataAggregator>()
    .AddSingleton<ApplicationEventStream>()
    .AddSingleton<ILlmClient, LlmClient>()
    .AddHostedService(sp => sp.GetRequiredService<ILlmClient>())
    .AddSingleton<IDataStorage>(redisStore)
    .AddSingleton<IModelStorage>(redisStore)
    .AddSingleton<IModelService, ModelService>()
    .AddSingleton<IModelAnalyzer, ModelAnalyzer>()
    .AddSingleton<Settings>();

var app = builder.Build();

app.UsePathBase("/api");
app.UseCors();
app.MapControllers();

await app.RunAsync();

return;

internal enum ClientType
{
    Lib,
    Grpc,
}
