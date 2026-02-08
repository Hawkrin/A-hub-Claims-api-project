using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Application.Services;
using ASP.Claims.API.Infrastructure.Messaging;
using StackExchange.Redis;

namespace ASP.Claims.API.Extensions;

public static class MessagingExtensions
{
    /// <summary>
    /// Configures event publishing with Redis or no-op fallback
    /// </summary>
    public static IServiceCollection AddEventPublishing(
        this IServiceCollection services, 
        IConfiguration configuration, 
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            // Development: Aspire provides Redis container via builder.AddRedisClient()
            // The IConnectionMultiplexer is already registered by Aspire
            services.AddSingleton<IEventPublisher, RedisEventPublisher>();
        }
        else
        {
            // Production: Use Azure Redis Cache or no-op if not configured
            var redisConnectionString = configuration.GetConnectionString("Redis");
            
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
                    try
                    {
                        return ConnectionMultiplexer.Connect(redisConnectionString);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to connect to Redis. Event publishing will be disabled.");
                        return null!;
                    }
                });
                
                services.AddSingleton<IEventPublisher, RedisEventPublisher>();
            }
            else
            {
                // No Redis configured - use no-op publisher
                services.AddSingleton<IEventPublisher, NoOpEventPublisher>();
            }
        }

        // Register the claim event publisher service
        services.AddScoped<IClaimEventPublisher, ClaimEventPublisher>();

        return services;
    }
}
