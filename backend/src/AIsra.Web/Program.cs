using System.ClientModel;
using System.Diagnostics;
using System.IO.Pipelines;
using DotNetEnv;
using AIsra.Web.Services;
using AIsra.Web.Services.Chat;
using AIsra.Web.Services.Data;
using AIsra.Web.Services.Mcp.Client;
using AIsra.Web.Services.Modeling;
using AIsra.Web.Services.Persistence;
using AIsra.HeuristicLibAdapter;
using AIsra.HeuristicLibAdapter.Grpc;
using AIsra.Common.Random;
using AIsra.Common.Threading;
using AIsra.HeuristicLibAdapter.Lib;
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

Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

var services = builder.Services;

builder.WebHost.ConfigureKestrel(options =>
{
    var httpPort = int.Parse(cfg["AGENT_PORT"].NotBlankOrThrow(
        new InvalidOperationException("AGENT_PORT environment variable not set.")
    ));

    options.ListenAnyIP(httpPort, listenOptions =>
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

cfg["CLIENT_TYPE"] ??= "Grpc";
cfg["MODEL_ENDPOINT"] ??= "https://openrouter.ai/api/v1/";
cfg["MODEL"] ??= "openrouter/free";

if (!Enum.TryParse<ClientType>(cfg["CLIENT_TYPE"], true, out var clientType))
{
    throw new InvalidOperationException(
        "Invalid ClientType configuration. " +
        $"ClientType must be one of: {string.Join(", ", Enum.GetNames<ClientType>())}."
    );
}

var apiKey = cfg["LLM_API_KEY"].NotBlankOrThrow(
    new InvalidOperationException(
        "OpenRouter API key not set. " +
        "Please set the LLM_API_KEY environment variable."
    )
);

if (string.IsNullOrWhiteSpace(cfg["MODEL"]))
{
    throw new InvalidOperationException("Model not specified in configuration.");
}

var openAiChatClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri(cfg["MODEL_ENDPOINT"]!),
    }
).GetChatClient(cfg["MODEL"]).AsIChatClient();

var chatClient = new ChatClientBuilder(openAiChatClient)
    .UseFunctionInvocation()
    .Build();

await using var cs = new CancellationService();
await using var redisStore = new RedisStorage((cfg["REDIS_HOST"] ?? "localhost") + ":" + (cfg["REDIS_PORT"] ?? "6379"));
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
                cfg["HEURISTIC_LIB_SERVER_URL"]
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
