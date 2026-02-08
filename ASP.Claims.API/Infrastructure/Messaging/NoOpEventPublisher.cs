using ASP.Claims.API.Application.Interfaces;

namespace ASP.Claims.API.Infrastructure.Messaging;

/// <summary>
/// No-op event publisher used when Redis is not configured (e.g., production without event bus).
/// Logs events but does not publish them to any message broker.
/// </summary>
public class NoOpEventPublisher(ILogger<NoOpEventPublisher> logger) : IEventPublisher
{
    private readonly ILogger<NoOpEventPublisher> _logger = logger;

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        var eventType = typeof(T).Name;
        
        _logger.LogWarning(
            "Event publishing is disabled (Redis not configured). Event {EventType} was not published: {@Event}",
            eventType, 
            @event
        );
        
        return Task.CompletedTask;
    }
}
