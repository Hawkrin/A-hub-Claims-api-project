using ASP.Claims.ServiceDefaults.Events;
using StackExchange.Redis;
using System.Text.Json;

namespace ASP.Claims.NotificationsWorker;

public class Worker(ILogger<Worker> logger, IConnectionMultiplexer redis) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IConnectionMultiplexer _redis = redis;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationsWorker starting up...");

        var subscriber = _redis.GetSubscriber();

        // Subscribe to ClaimEscalatedEvent
        await subscriber.SubscribeAsync(
            RedisChannel.Literal(nameof(ClaimEscalatedEvent)),
            async (channel, message) =>
            {
                try
                {
                    var eventData = JsonSerializer.Deserialize<ClaimEscalatedEvent>(message!);
                    if (eventData != null)
                    {
                        await HandleClaimEscalatedAsync(eventData, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing ClaimEscalatedEvent");
                }
            });

        // Subscribe to ClaimFraudFlaggedEvent
        await subscriber.SubscribeAsync(
            RedisChannel.Literal(nameof(ClaimFraudFlaggedEvent)),
            async (channel, message) =>
            {
                try
                {
                    var eventData = JsonSerializer.Deserialize<ClaimFraudFlaggedEvent>(message!);
                    if (eventData != null)
                    {
                        await HandleClaimFraudFlaggedAsync(eventData, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing ClaimFraudFlaggedEvent");
                }
            });

        _logger.LogInformation("NotificationsWorker subscribed to events and running");

        // Keep the worker alive
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("NotificationsWorker shutting down");
    }

    private async Task HandleClaimEscalatedAsync(ClaimEscalatedEvent eventData, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "?? NOTIFICATION: High-value claim escalation detected!\n" +
            "   ClaimId: {ClaimId}\n" +
            "   Type: {ClaimType}\n" +
            "   Amount: {Amount:C}\n" +
            "   Address: {Address}\n" +
            "   Time: {OccurredAt}",
            eventData.ClaimId,
            eventData.ClaimType,
            eventData.Amount,
            eventData.Address,
            eventData.OccurredAt
        );

        // TODO: Send actual notification (email, SMS, push notification)
        // For now, just log it
        await Task.CompletedTask;
    }

    private async Task HandleClaimFraudFlaggedAsync(ClaimFraudFlaggedEvent eventData, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "?? NOTIFICATION: Potential fraud detected!\n" +
            "   ClaimId: {ClaimId}\n" +
            "   Type: {ClaimType}\n" +
            "   Address: {Address}\n" +
            "   Reason: {FraudReason}\n" +
            "   Time: {OccurredAt}",
            eventData.ClaimId,
            eventData.ClaimType,
            eventData.Address,
            eventData.FraudReason,
            eventData.OccurredAt
        );

        // TODO: Send fraud alert notification
        await Task.CompletedTask;
    }
}
