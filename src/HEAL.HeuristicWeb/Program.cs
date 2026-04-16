using HEAL.HeuristicGrpc.Server;
using HEAL.HeuristicRest.Services.Storage;
using HEAL.HeuristicRest.Util;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi()
    .AddControllers();
builder.Services
    .AddSingleton<SolutionStore>()
    .AddSwaggerGen(opt => opt.IncludeXmlComments(AssemblyUtil.XmlPath));
builder.Services.AddGrpc();

var app = builder.Build();

app.AddHeuristicGrpc();

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
