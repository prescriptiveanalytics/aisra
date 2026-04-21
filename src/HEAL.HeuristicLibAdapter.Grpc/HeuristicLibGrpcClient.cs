using Grpc.Net.Client;
using HEAL.HeuristicGrpc.Core.Proto;
using HEAL.HeuristicLibContracts.Dtos;
using HEAL.HeuristicWeb.Grpc.Core.Mapping;

namespace HEAL.HeuristicLibAdapter.Grpc;

public class HeuristicLibGrpcClient : IHeuristicLibClient
{
    private readonly GrpcChannel _channel;
    private GrpcHeuristicService.GrpcHeuristicServiceClient _client;

    public HeuristicLibGrpcClient(string address)
    {
        _channel = GrpcChannel.ForAddress(address);
        _client = new(_channel);
    }

    ~HeuristicLibGrpcClient() => Dispose();

    public async Task<double[]> RunBenchmarkAsync(BenchmarkHyperparametersDto dto, CancellationToken ct = default)
        => (await _client.RunBenchmarkAsync(dto.ToGrpc(), cancellationToken: ct)).Values.ToArray();

    public async Task<string> RunSymRegAsync(SymbolicRegressionHyperparametersDto dto, CancellationToken ct = default)
        => (await _client.FitAsync(dto.ToGrpc(), cancellationToken: ct)).Expression;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _channel.Dispose();
    }
}
