using System.Text.Json;
using AIsra.Web.Services.Reset;
using DsgWeb.Client;

namespace AIsra.Web.Services.DataGeneration;

/// <summary>
/// Functionality on top of the dsg-web API: the default configuration, and restarting and stopping the stream.
/// </summary>
public sealed class DataGeneratorService : IResettable
{
    private static readonly TimeSpan StopTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan StopPollInterval = TimeSpan.FromMilliseconds(250);

    private readonly IDsgWebClient client;
    private readonly ILogger<DataGeneratorService> logger;
    private readonly string defaultConfigurationJson;

    public DataGeneratorService(IDsgWebClient client, IConfiguration config, ILogger<DataGeneratorService> logger)
    {
        this.client = client;
        this.logger = logger;

        var defaultConfig = Deserialize(
            typeof(DataGeneratorService).Assembly
                .Let(asm => asm.ReadEmbeddedTextFile($"{asm.GetName().Name}.Resources.DefaultDataGeneratorConfig.json"))
        );
        var generator = defaultConfig.Generator!;

        if (config["DSG_BROKER_HOST"] is { Length: > 0 } brokerHost)
        {
            generator.BrokerHostName = brokerHost;
        }

        if (int.TryParse(config["DSG_BROKER_PORT"], out var brokerPort))
        {
            generator.BrokerHostPort = brokerPort;
        }

        defaultConfigurationJson = JsonSerializer.Serialize(defaultConfig, JsonSerializerOptions.Web);
    }

    /// <summary>
    /// Gets a new copy of the built-in default configuration.
    /// </summary>
    public DsgConfigurationResponse GetDefaultConfiguration() => Deserialize(defaultConfigurationJson);

    /// <summary>
    /// Stops the stream if it is running, waits until it has stopped, and starts it again.
    /// </summary>
    /// <exception cref="TimeoutException">The stream did not stop in time.</exception>
    public async Task<StartStreamResponse> RestartAsync(CancellationToken ct = default)
    {
        await StopAndWaitAsync(ct);

        return await client.StartStreamAsync(ct);
    }

    /// <summary>
    /// Stops the stream so that no data arrives after the reset.
    /// </summary>
    /// <remarks>
    /// The reset continues if the generator is unavailable, since the rest of the application can still be reset.
    /// </remarks>
    public async Task ResetAsync(CancellationToken ct)
    {
        try
        {
            await StopAndWaitAsync(ct);
        }
        catch (Exception e) when (e is ApiException or HttpRequestException or TimeoutException)
        {
            logger.LogWarning(e, "The data stream could not be stopped while resetting the application.");
        }
    }

    /// <summary>
    /// Stops the stream if it is running and waits until it has stopped.
    /// </summary>
    /// <exception cref="TimeoutException">The stream did not stop in time.</exception>
    private async Task StopAndWaitAsync(CancellationToken ct)
    {
        if (await IsRunningAsync(ct))
        {
            try
            {
                await client.StopStreamAsync(ct);
            }
            catch (ApiException e) when (e.StatusCode == StatusCodes.Status409Conflict)
            {
                // The stream stopped on its own in the meantime.
            }

            // Stopping is asynchronous, so wait until the generator reports that it has stopped.
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(StopTimeout);

            try
            {
                do
                {
                    await Task.Delay(StopPollInterval, timeout.Token);
                } while (await IsRunningAsync(timeout.Token));
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new TimeoutException("The data stream did not stop in time.");
            }
        }
    }

    private async Task<bool> IsRunningAsync(CancellationToken ct)
        => (await client.GetStatusAsync(ct)).Status == "running";

    private static DsgConfigurationResponse Deserialize(string json)
        => JsonSerializer.Deserialize<DsgConfigurationResponse>(json, JsonSerializerOptions.Web)!;
}
