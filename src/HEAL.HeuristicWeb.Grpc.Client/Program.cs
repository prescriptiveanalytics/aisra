using Grpc.Net.Client;
using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicWeb.Grpc.Core.Mapping;
using HEAL.HeuristicLibContracts.Dtos;

using var channel = GrpcChannel.ForAddress("https://localhost:7017");
var client = new GrpcHeuristicService.GrpcHeuristicServiceClient(channel);
var reply = await client.RunBenchmarkAsync(
        new FuncProblemDto
        {
            Function = "Sphere",
            Dimensions = 5,
        }.ToGrpc()
);
Console.WriteLine("Results: " + reply.Values);
