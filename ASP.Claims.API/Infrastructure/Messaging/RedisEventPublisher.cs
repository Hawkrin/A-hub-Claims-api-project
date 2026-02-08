using ASP.Claims.API.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace ASP.Claims.API.Infrastructure.Messaging;

public class RedisEventPublisher(IConnectionMultiplexer redis, ILogger<RedisEventPublisher> logger) : IEventPublisher
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly ILogger<RedisEventPublisher> _logger = logger;

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        var channel = typeof(T).Name;
        var message = JsonSerializer.Serialize(@event);

        try
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal(channel), message);
            
            _logger.LogInformation("Published event {EventType} to channel {Channel}", typeof(T).Name, channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to channel {Channel}", typeof(T).Name, channel);
            throw;
        }
    }
}
