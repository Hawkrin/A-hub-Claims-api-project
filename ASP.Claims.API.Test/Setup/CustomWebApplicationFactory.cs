using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ASP.Claims.API.Application.Interfaces;
using Moq;

namespace ASP.Claims.API.Test.Setup;

/// <summary>
/// Overrides the default WebApplicationFactory to configure test-specific services.
/// Environment is set to "Test", which triggers in-memory repository registration in ServiceCollectionExtensions.
/// This factory only needs to handle test-specific concerns: authentication and event publisher mocking.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to "Test" - this triggers in-memory repositories in ServiceCollectionExtensions
        builder.UseEnvironment("Test");

        builder.ConfigureServices((context, services) =>
        {
            // ============================================================================
            // TEST AUTHENTICATION (Replace JWT with Test Auth)
            // ============================================================================
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
                options.DefaultChallengeScheme = TestAuthHandler.TestScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, options => { });

            // ============================================================================
            // MOCK EVENT PUBLISHERS (Redis not available in tests)
            // ============================================================================
            
            // Remove and replace IEventPublisher with mock
            RemoveAndReplaceService(services, CreateMockEventPublisher());

            // Remove and replace IClaimEventPublisher with mock
            RemoveAndReplaceService(services, CreateMockClaimEventPublisher());

            // ============================================================================
            // REMOVE REDIS CLIENT (Not needed for tests)
            // ============================================================================
            RemoveService(services, d => 
                d.ServiceType.Name.Contains("IConnectionMultiplexer") || 
                d.ImplementationType?.Name.Contains("Redis") == true);
        });
    }

    /// <summary>
    /// Creates a mock IEventPublisher that does nothing (no Redis dependency)
    /// </summary>
    private static IEventPublisher CreateMockEventPublisher()
    {
        var mock = new Mock<IEventPublisher>();
        mock.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    /// <summary>
    /// Creates a mock IClaimEventPublisher that does nothing (no Redis dependency)
    /// </summary>
    private static IClaimEventPublisher CreateMockClaimEventPublisher()
    {
        var mock = new Mock<IClaimEventPublisher>();
        mock.Setup(x => x.PublishClaimEventsAsync(
            It.IsAny<Domain.Entities.Claim>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    /// <summary>
    /// Helper method to remove all registrations of a service type and add a specific instance
    /// </summary>
    private static void RemoveAndReplaceService<TService>(
        IServiceCollection services, 
        TService implementation) 
        where TService : class
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(TService)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
        services.AddSingleton(implementation);
    }

    /// <summary>
    /// Helper method to remove services matching a predicate
    /// </summary>
    private static void RemoveService(
        IServiceCollection services, 
        Func<ServiceDescriptor, bool> predicate)
    {
        var descriptors = services.Where(predicate).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }
}
