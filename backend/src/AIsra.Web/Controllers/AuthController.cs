using AIsra.Web.Dtos;
using AIsra.Web.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIsra.Web.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController(AuthSettings settings) : ControllerBase
{
    /// <summary>
    /// Returns the signed-in user, 401 if nobody is signed in or 403 if the user is not authorized.
    /// </summary>
    [HttpGet("me")]
    public ActionResult<UserDto> Me()
    {
        if (!settings.Enabled)
        {
            return new UserDto
            {
                AuthenticationEnabled = false,
            };
        }

        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        if (!settings.IsAuthorized(User))
        {
            return Forbid();
        }

        return new UserDto
        {
            AuthenticationEnabled = true,
            Name = User.Identity.Name,
            Email = AuthSettings.GetEmail(User),
        };
    }

    /// <summary>
    /// Starts the Google sign-in and redirects back to <paramref name="returnUrl"/> afterward.
    /// </summary>
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl)
    {
        if (!settings.Enabled)
        {
            return NotFound();
        }

        return Challenge(
            new AuthenticationProperties { RedirectUri = settings.ResolveReturnUrl(returnUrl) },
            OpenIdConnectDefaults.AuthenticationScheme
        );
    }

    /// <summary>
    /// Ends the session and redirects back to the frontend.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (!settings.Enabled)
        {
            return NotFound();
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect(settings.FrontendUrl!.ToString());
    }
}
