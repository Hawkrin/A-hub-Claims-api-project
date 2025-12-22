using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Infrastructures.Repositories;

namespace ASP.Claims.API.Test.Setup;

/// <summary>
/// Overrides the default WebApplicationFactory to configure test authentication and force in-memory repository.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
                options.DefaultChallengeScheme = TestAuthHandler.TestScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, options => { });

            // Remove all IClaimRepository registrations
            var claimDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClaimRepository));
            if (claimDescriptor != null)
                services.Remove(claimDescriptor);
            // Register InMemoryClaimRepository
            services.AddSingleton<IClaimRepository, InMemoryClaimRepository>();

            // Remove all IUserRepository registrations
            var userDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserRepository));
            if (userDescriptor != null)
                services.Remove(userDescriptor);
            // Register InMemoryUserRepository
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        });
    }
}