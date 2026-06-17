namespace AIsra.Web.Services.Chat;

public interface ILlmClient : IHostedService
{
    bool AgentIsBusy { get; }

    Task ChatAsync(string message, CancellationToken ct);
}
