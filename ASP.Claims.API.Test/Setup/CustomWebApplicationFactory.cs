using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Infrastructures.Repositories;

namespace ASP.Claims.API.Test.Setup;

/// <summary>
/// Overrides the default WebApplicationFactory to configure test authentication.
/// Uses Cosmos emulator if Cosmos config is present; otherwise forces in-memory repositories.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices((context, services) =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
                options.DefaultChallengeScheme = TestAuthHandler.TestScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, options => { });

            var cfg = context.Configuration;
            var cosmosConfigured = !string.IsNullOrWhiteSpace(cfg["CosmosDb:Account"])
                && !string.IsNullOrWhiteSpace(cfg["CosmosDb:DatabaseName"])
                && !string.IsNullOrWhiteSpace(cfg["CosmosDb:Containers:Claims"])
                && !string.IsNullOrWhiteSpace(cfg["CosmosDb:Containers:Users"]);

            if (cosmosConfigured)
                return;

            // Fallback to in-memory for tests when Cosmos isn't configured

            var claimDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClaimRepository));
            if (claimDescriptor != null)
                services.Remove(claimDescriptor);
            services.AddSingleton<IClaimRepository, InMemoryClaimRepository>();

            var userDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserRepository));
            if (userDescriptor != null)
                services.Remove(userDescriptor);
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        });
    }
}