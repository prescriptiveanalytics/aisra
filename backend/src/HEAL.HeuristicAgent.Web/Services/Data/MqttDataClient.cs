using System.Text.Json;
using HEAL.HeuristicAgent.Web.Services.Persistence;
using HEAL.HeuristicLibContracts.Threading;
using MQTTnet;
using MQTTnet.Protocol;

namespace HEAL.HeuristicAgent.Web.Services.Data;

public sealed class MqttDataClient : IDataClient, IDisposable
{
    private readonly IMqttClient client;
    private readonly SortedDictionary<string, double> latestValues = new();
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
    private static readonly TimeSpan Interval = 0.5.Seconds;

    public MqttDataClient(IDataStorage dataStorage, ICancellationTokenProvider ctp)
    {
        const string host = "localhost";
        const int port = 1883;
        const string topicFilter = "resource1/raw/#";

        var ct = ctp.Token;

        var factory = new MqttClientFactory();
        client = factory.CreateMqttClient();
        client.ConnectedAsync += async _ =>
        {
            await client.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topicFilter)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build());

            Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    var delayTask = Interval.WithCancellationToken(ct);

                    var data = latestValues.Values.ToArray();

                    if (data.Length >= 3)
                    {
                        await dataStorage.InsertAsync(data);
                        DataReceived?.Invoke(this, data);
                    }

                    await delayTask;
                }
            }).Forget();
        };
        client.ApplicationMessageReceivedAsync += e =>
        {
            try
            {
                var payload = e.ApplicationMessage.ConvertPayloadToString();
                var dto = JsonSerializer.Deserialize<DataPointDto>(payload, jsonOptions);

                if (dto is null)
                {
                    Console.WriteLine($"Received invalid MQTT message on topic {e.ApplicationMessage.Topic}: {payload}");

                    return Task.CompletedTask;
                }

                latestValues[dto.Id] = dto.Value;

                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        };
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithCleanSession();
        var options = optionsBuilder.Build();
        client.ConnectAsync(options);
    }

    ~MqttDataClient()
    {
        Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        client.Dispose();
    }

    public event EventHandler<double[]>? DataReceived;

    public sealed record DataPointDto(string Id, double Value);
}
