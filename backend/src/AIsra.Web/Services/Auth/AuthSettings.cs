using System.Security.Claims;
using System.Text.RegularExpressions;

namespace AIsra.Web.Services.Auth;

/// <summary>
/// Authentication configuration read from the environment.
/// </summary>
public sealed class AuthSettings
{
    public required bool Enabled { get; init; }

    public Regex? AuthorizedUsers { get; init; }

    public Uri? FrontendUrl { get; init; }

    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }

    public static AuthSettings FromConfiguration(IConfiguration cfg)
    {
        var useAuthentication = cfg["USE_AUTHENTICATION"];

        bool enabled;

        if (string.IsNullOrWhiteSpace(useAuthentication))
        {
            enabled = true;
        }
        else if (!bool.TryParse(useAuthentication, out enabled))
        {
            throw new InvalidOperationException("Invalid USE_AUTHENTICATION configuration. Must be true or false.");
        }

        var frontendUrl = string.IsNullOrWhiteSpace(cfg["FRONTEND_URL"])
            ? null
            : new Uri(cfg["FRONTEND_URL"]!, UriKind.Absolute);

        if (!enabled)
        {
            return new AuthSettings
            {
                Enabled = false,
                FrontendUrl = frontendUrl,
            };
        }

        var authorizedUsersRegex = cfg["AUTHORIZED_USERS_REGEX"].NotBlankOrThrow(
            new InvalidOperationException(
                "AUTHORIZED_USERS_REGEX environment variable not set. " +
                "It is required when USE_AUTHENTICATION is enabled."
            )
        );

        return new AuthSettings
        {
            Enabled = true,
            AuthorizedUsers = new Regex(
                authorizedUsersRegex,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(1)
            ),
            FrontendUrl = frontendUrl ?? throw new InvalidOperationException(
                "FRONTEND_URL environment variable not set. It is required when USE_AUTHENTICATION is enabled."
            ),
            ClientId = cfg["GOOGLE_CLIENT_ID"].NotBlankOrThrow(
                new InvalidOperationException("GOOGLE_CLIENT_ID environment variable not set.")
            ),
            ClientSecret = cfg["GOOGLE_CLIENT_SECRET"].NotBlankOrThrow(
                new InvalidOperationException("GOOGLE_CLIENT_SECRET environment variable not set.")
            ),
        };
    }

    /// <summary>
    /// Returns the user's email address, or <see langword="null"/> if Google has not verified it.
    /// </summary>
    public static string? GetEmail(ClaimsPrincipal? user)
    {
        var email = user?.FindFirst("email")?.Value;
        var emailVerified = user?.FindFirst("email_verified")?.Value;

        return !string.IsNullOrWhiteSpace(email)
               && string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase)
            ? email
            : null;
    }

    /// <summary>
    /// Checks whether the user is allowed to use the application.
    /// Always <see langword="true"/> if authentication is disabled.
    /// </summary>
    public bool IsAuthorized(ClaimsPrincipal? user)
    {
        if (!Enabled)
        {
            return true;
        }

        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var email = GetEmail(user);

        return email is not null && AuthorizedUsers!.IsMatch(email);
    }

    /// <summary>
    /// Returns <paramref name="returnUrl"/> if it points to the frontend, otherwise the frontend's base URL.
    /// Prevents open redirects.
    /// </summary>
    public string ResolveReturnUrl(string? returnUrl)
    {
        var fallback = FrontendUrl!.ToString();

        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
        {
            return fallback;
        }

        return Uri.Compare(
            uri, FrontendUrl, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase
        ) == 0
            ? uri.ToString()
            : fallback;
    }

    public string FrontendUrlWithError(string error)
        => new UriBuilder(FrontendUrl!) { Query = $"authError={Uri.EscapeDataString(error)}" }.ToString();
}
