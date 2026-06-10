using System.Text.Json;
using HEAL.HeuristicAgent.Web.Dtos;
using MQTTnet;
using MQTTnet.Protocol;

namespace HEAL.HeuristicAgent.Web.Services.Data;

public sealed class MqttDataClient : IDataClient, IDisposable
{
    private readonly IMqttClient client;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public MqttDataClient(IConfiguration config, IDataAggregator dataAggregator)
    {
        var host = config["MqttHost"] ?? "localhost";
        var port = int.Parse(config["MqttPort"] ?? "1883");
        var topicFilter = config["MqttTopicFilter"] ?? "#";

        var factory = new MqttClientFactory();
        client = factory.CreateMqttClient();
        client.ConnectedAsync += async _ =>
        {
            await client.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topicFilter)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build());
        };
        client.ApplicationMessageReceivedAsync += e =>
        {
            try
            {
                var payload = e.ApplicationMessage.ConvertPayloadToString();
                var dataPoint = JsonSerializer.Deserialize<DataPointDto>(payload, jsonOptions);

                if (dataPoint is null)
                {
                    Console.WriteLine($"Received invalid MQTT message on topic {e.ApplicationMessage.Topic}: {payload}");

                    return Task.CompletedTask;
                }

                dataAggregator.Push(dataPoint);
                DataReceived?.Invoke(this, dataPoint);

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

    public event EventHandler<DataPointDto>? DataReceived;
}
