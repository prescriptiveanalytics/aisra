using HEAL.HeuristicWeb.Server.GRpc;
using HEAL.HeuristicWeb.Server.Rest.Services.Storage;
using HEAL.HeuristicWeb.Server.Rest.Util;

var builder = WebApplication.CreateBuilder(args);

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
