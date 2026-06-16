using HEAL.HeuristicAgent.Web.Services;
using HEAL.HeuristicLibContracts.Random;
using HEAL.HeuristicLibContracts.Threading;
using HEAL.HeuristicWeb.Server.Grpc;
using HEAL.HeuristicWeb.Server.Rest.Storage;
using HEAL.HeuristicWeb.Server.Rest.Util;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opt =>
{
    var cfg = builder.Configuration;
    var grpcPort = int.Parse(cfg["GRPC_PORT"] ?? "5000");
    var restPort = int.Parse(cfg["REST_PORT"] ?? "5001");

    opt.ListenAnyIP(grpcPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });

    opt.ListenAnyIP(restPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

await using var ctp = new CancellationService();
var rng = new Rng();

builder.Services.AddOpenApi()
    .AddControllers();
builder.Services
    .AddSingleton<SolutionStore>()
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
