using DotNetEnv;
using AIsra.Common.Random;
using AIsra.Common.Threading;
using AIsra.HeuristicLibWeb.Server.Grpc;
using AIsra.HeuristicLibWeb.Server.Rest.Storage;
using AIsra.HeuristicLibWeb.Server.Rest.Util;
using Microsoft.AspNetCore.Server.Kestrel.Core;

Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opt =>
{
    var cfg = builder.Configuration;
    var restPort = int.TryParse(cfg["REST_PORT"], out var parsedPort) ? parsedPort : 5000;
    var grpcPort = int.TryParse(cfg["GRPC_PORT"], out parsedPort) ? parsedPort : 5001;

    opt.ListenAnyIP(restPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });

    opt.ListenAnyIP(grpcPort, listenOptions =>
    {
        listenOptions.UseHttps();
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

await using var ctp = new CancellationService();
var rng = new Rng();

builder.Services
    .AddOpenApi()
    .AddControllers();
builder.Services
    .AddSingleton<SolutionStorage>()
    .AddSingleton<IRng>(rng)
    .AddSingleton<ICancellationTokenProvider>(ctp)
    .AddSwaggerGen(opt => opt.IncludeXmlComments(AssemblyUtil.XmlPath));
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<HeuristicService>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UsePathBase("/api");
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

await app.RunAsync();
