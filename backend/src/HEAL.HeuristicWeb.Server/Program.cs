using HEAL.HeuristicWeb.Server.Grpc;
using HEAL.HeuristicWeb.Server.Rest.Services.Storage;
using HEAL.HeuristicWeb.Server.Rest.Util;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(opt =>
{
    opt.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    opt.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps();
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
