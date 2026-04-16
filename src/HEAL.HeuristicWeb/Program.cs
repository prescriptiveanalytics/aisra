using HEAL.HeuristicGrpc.Server;
using HEAL.HeuristicRest.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi()
    .AddControllers();
builder.Services
    .AddSingleton<SolutionStore>()
    .AddSwaggerGen();
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
