using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Contribution.Common.Auth;

public class CommonAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        (var authScheme, var token) = AuthHelpers.ExtractAuthDetails(Request.Headers.Authorization.ToString() ?? string.Empty) ?? (string.Empty, string.Empty);

        if (string.IsNullOrWhiteSpace(authScheme))
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authentication Scheme"));

        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(AuthenticateResult.Fail("Missing or empty token"));

        var claims = new[] { new Claim(ClaimTypes.Name, token) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
