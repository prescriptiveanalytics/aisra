using System.Text.Json;
using AIsra.Web.Dtos;
using MQTTnet;
using MQTTnet.Protocol;

namespace AIsra.Web.Services.Data;

public sealed class MqttDataClient : IDataClient, IDisposable
{
    private readonly IMqttClient client;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public MqttDataClient(IConfiguration config, IDataAggregator dataAggregator)
    {
        var host = config["MQTT_HOST"] ?? "localhost";
        var port = int.TryParse(config["MQTT_HOST_PORT"], out var parsedPort) ? parsedPort : 1883;
        var topicFilter = config["MQTT_TOPIC_FILTER"] ?? "#";

        var factory = new MqttClientFactory();
        client = factory.CreateMqttClient();
        client.ConnectedAsync += async _ =>
        {
            await client.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topicFilter)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
        };
        client.ApplicationMessageReceivedAsync += e =>
        {
            try
            {
                var payload = e.ApplicationMessage.ConvertPayloadToString();
                var dataPoint = JsonSerializer.Deserialize<DataPointDto>(payload, jsonOptions);
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
