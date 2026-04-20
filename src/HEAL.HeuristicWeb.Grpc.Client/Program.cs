// #define SYMREG

using Grpc.Net.Client;
using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicWeb.Grpc.Core.Mapping;
using HEAL.HeuristicLibContracts.Dtos;

using var channel = GrpcChannel.ForAddress("https://localhost:7017");
var client = new GrpcHeuristicService.GrpcHeuristicServiceClient(channel);

#if SYMREG
var reply = await client.FitAsync(
        new SymRegProblemDto
        {
            Problem = new(),
            VariableNames = ["x1", "x2", "y"],
            Data = [
                [1, 1, 1],
                [1, 2, 2],
                [2, 1, 2],
                [2, 2, 4],
                [3, 3, 9],
                [2, 3, 6],
                [3, 2, 6]
            ],
        }.ToGrpc()
);
Console.WriteLine("Results: " + reply.Expression);
#else
var reply = await client.RunBenchmarkAsync(
        new FuncProblemDto
        {
            Problem = new(),
            Function = "Sphere",
            Dimensions = 5,
        }.ToGrpc()
);
Console.WriteLine("Results: " + reply.Values);
#endif
