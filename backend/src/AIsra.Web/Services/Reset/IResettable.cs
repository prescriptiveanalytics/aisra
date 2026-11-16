namespace AIsra.Web.Services.Reset;

/// <summary>
/// A service with state that is reset by <see cref="ApplicationResetService"/>.
/// </summary>
/// <remarks>
/// Register implementations with <see cref="ResetServiceCollectionExtensions.AddResettable{T}"/>.
/// </remarks>
public interface IResettable
{
    /// <summary>
    /// Restores the state the service had when the application started.
    /// </summary>
    /// <remarks>The agent is suspended while this is called.</remarks>
    Task ResetAsync(CancellationToken ct);
}
