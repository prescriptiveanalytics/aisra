using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AIsra.Web.Services.Auth;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Registers Google sign-in with a session cookie and requires an authorized user
    /// for every endpoint that is not explicitly marked with <see cref="AllowAnonymousAttribute"/>.
    /// </summary>
    public static IServiceCollection AddAisraAuthentication(this IServiceCollection services, AuthSettings settings)
    {
        services.AddSingleton(settings);

        if (!settings.Enabled)
        {
            services.AddAuthorization();
            return services;
        }

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // API clients get a 401 instead of a redirect; the frontend starts the sign-in via /auth/login.
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "aisra.auth";
                options.Cookie.HttpOnly = true;
                // The frontend runs on a different origin, so the cookie must be sent cross-site.
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;

                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = "https://accounts.google.com";
                options.ClientId = settings.ClientId;
                options.ClientSecret = settings.ClientSecret;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.CallbackPath = "/signin-oidc";
                options.MapInboundClaims = false;
                options.TokenValidationParameters.NameClaimType = "name";
                // Google issues ID tokens with either form of the issuer.
                options.TokenValidationParameters.ValidIssuers = ["https://accounts.google.com", "accounts.google.com"];

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    context.ProtocolMessage.Prompt = "select_account";
                    return Task.CompletedTask;
                };

                options.Events.OnTokenValidated = context =>
                {
                    if (!settings.IsAuthorized(context.Principal))
                    {
                        context.HandleResponse();
                        context.Response.Redirect(settings.FrontendUrlWithError("forbidden"));
                    }

                    return Task.CompletedTask;
                };

                options.Events.OnRemoteFailure = context =>
                {
                    context.HandleResponse();
                    context.Response.Redirect(settings.FrontendUrlWithError("failed"));
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireAssertion(context => settings.IsAuthorized(context.User))
                .Build()
            );

        return services;
    }
}
