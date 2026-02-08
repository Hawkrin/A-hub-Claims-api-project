using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Infrastructures.Repositories;
using Moq;

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
            // Add test authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
                options.DefaultChallengeScheme = TestAuthHandler.TestScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, options => { });

            // Remove Redis client registration (not needed for tests)
            var redisDescriptor = services.FirstOrDefault(d => 
                d.ServiceType.Name.Contains("IConnectionMultiplexer") || 
                d.ImplementationType?.Name.Contains("Redis") == true);
            if (redisDescriptor != null)
                services.Remove(redisDescriptor);

            // Mock IEventPublisher to prevent Redis dependency in tests
            var eventPublisherDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEventPublisher));
            if (eventPublisherDescriptor != null)
                services.Remove(eventPublisherDescriptor);
            
            var mockEventPublisher = new Mock<IEventPublisher>();
            mockEventPublisher
                .Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            services.AddSingleton(mockEventPublisher.Object);

            // Mock IClaimEventPublisher as well
            var claimEventPublisherDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClaimEventPublisher));
            if (claimEventPublisherDescriptor != null)
                services.Remove(claimEventPublisherDescriptor);
            
            var mockClaimEventPublisher = new Mock<IClaimEventPublisher>();
            mockClaimEventPublisher
                .Setup(x => x.PublishClaimEventsAsync(It.IsAny<Domain.Entities.Claim>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            services.AddSingleton(mockClaimEventPublisher.Object);

            var cfg = context.Configuration;
            var cosmosConfigured = !string.IsNullOrWhiteSpace(cfg["CosmosDb:Account"])
                && !string.IsNullOrWhiteSpace(cfg["CosmosDb:DatabaseName"])
                && !string.IsNullOrWhiteSpace(cfg["CosmosDb:Containers:Claims"])
                && !string.IsNullOrWhiteSpace(cfg["CosmosDb:Containers:Users"]);

            if (cosmosConfigured)
                return;

            // Fallback to in-memory for tests when Cosmos isn't configured
            // Use Scoped instead of Singleton to isolate tests

            var claimDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClaimRepository));
            if (claimDescriptor != null)
                services.Remove(claimDescriptor);
            services.AddScoped<IClaimRepository, InMemoryClaimRepository>();

            var userDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUserRepository));
            if (userDescriptor != null)
                services.Remove(userDescriptor);
            services.AddScoped<IUserRepository, InMemoryUserRepository>();
        });
    }
}