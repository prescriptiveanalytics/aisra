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
    var enableHttps = bool.Parse(cfg["ENABLE_HTTPS"] ?? "true");

    opt.ListenAnyIP(grpcPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });

    opt.ListenAnyIP(restPort, listenOptions =>
    {
        if (enableHttps)
        {
            listenOptions.UseHttps();
        }

        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});
builder.Services.AddOpenApi()
    .AddControllers();
builder.Services
    .AddSingleton<SolutionStore>()
    .AddSwaggerGen(opt => opt.IncludeXmlComments(AssemblyUtil.XmlPath));
builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<HeuristicService>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UsePathBase("/api");
app.UseHttpsRedirection();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

await app.RunAsync();
