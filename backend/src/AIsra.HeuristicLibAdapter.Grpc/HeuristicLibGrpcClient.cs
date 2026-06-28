using Grpc.Net.Client;
using AIsra.HeuristicLibWeb.Grpc.Proto;
using AIsra.Common.Dtos;
using AIsra.HeuristicLibWeb.Grpc.Mapping;

namespace AIsra.HeuristicLibAdapter.Grpc;

public sealed class HeuristicLibGrpcClient : IHeuristicLibClient
{
    private readonly GrpcChannel channel;
    private readonly GrpcHeuristicService.GrpcHeuristicServiceClient client;

    public HeuristicLibGrpcClient(string address)
    {
        var httpHandler = new SocketsHttpHandler
        {
            SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                RemoteCertificateValidationCallback = (_, _, _, _) => true,
            },
        };
        channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
        {
            HttpHandler = httpHandler,
        });
        client = new(channel);
    }

    ~HeuristicLibGrpcClient() => Dispose();

    public async Task<double[]> RunBenchmarkAsync(BenchmarkHyperparametersDto dto, CancellationToken ct)
        => (await client.RunBenchmarkAsync(dto.ToGrpc(), cancellationToken: ct)).Values.ToArray();

    public async Task<string> RunSymRegAsync(SymbolicRegressionRequestDto dto, CancellationToken ct)
        => (await client.FitAsync(dto.ToGrpc(), cancellationToken: ct)).Expression;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        channel.Dispose();
    }
}
