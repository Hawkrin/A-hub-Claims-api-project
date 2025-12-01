using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ASP.Claims.API.Test.Setup;

/// <summary>
/// Provides a test authentication handler that issues a fixed identity for authentication scenarios, typically used in
/// integration testing or development environments.
/// </summary>
/// <remarks>This handler always authenticates requests as a user named "TestUser" with the "Admin" role using the
/// "TestScheme" scheme. It is intended for use in test environments and should not be used in production.</remarks>
/// <param name="options">The monitor used to retrieve authentication scheme options for the handler.</param>
/// <param name="logger">The factory used to create logger instances for logging within the handler.</param>
/// <param name="encoder">The encoder used to encode URLs as part of the authentication process.</param>
public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) :
    AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string TestScheme = "TestScheme";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "TestUser"), new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, TestScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}