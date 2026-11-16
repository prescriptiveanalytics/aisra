namespace AIsra.Web.Services.Reset;

public static class ResetServiceCollectionExtensions
{
    /// <summary>
    /// Registers the already registered service <typeparamref name="T"/> to be reset by
    /// <see cref="ApplicationResetService"/>. Services are reset in the order they are registered.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="T"/> keeps its own lifetime: it is wrapped in an adapter that is not disposable, because the
    /// scope of a reset would otherwise dispose the service, even if it is a singleton.
    /// </remarks>
    public static IServiceCollection AddResettable<T>(this IServiceCollection services)
        where T : class, IResettable
        => services.AddTransient<IResettable>(sp => new ResettableAdapter(sp.GetRequiredService<T>()));

    private sealed class ResettableAdapter(IResettable service) : IResettable
    {
        public Task ResetAsync(CancellationToken ct)
            => service.ResetAsync(ct);
    }
}
