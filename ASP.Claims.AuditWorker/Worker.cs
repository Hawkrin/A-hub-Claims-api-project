using ASP.Claims.AuditWorker.Interfaces;
using ASP.Claims.AuditWorker.Models;
using ASP.Claims.ServiceDefaults.Events;
using StackExchange.Redis;
using System.Text.Json;

namespace ASP.Claims.AuditWorker;

public class Worker(ILogger<Worker> logger, IConnectionMultiplexer redis, IAuditRepository auditRepository) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IAuditRepository _auditRepository = auditRepository;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuditWorker starting up...");

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
                        await AuditClaimEscalatedAsync(eventData, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error auditing ClaimEscalatedEvent");
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
                        await AuditClaimFraudFlaggedAsync(eventData, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error auditing ClaimFraudFlaggedEvent");
                }
            });

        // Subscribe to ClaimStatusChangedEvent
        await subscriber.SubscribeAsync(
            RedisChannel.Literal("ClaimStatusChangedEvent"),
            async (channel, message) =>
            {
                try
                {
                    var eventData = JsonSerializer.Deserialize<ClaimStatusChangedEvent>(message!);
                    if (eventData != null)
                    {
                        await AuditClaimStatusChangedAsync(eventData, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error auditing ClaimStatusChangedEvent");
                }
            });

        _logger.LogInformation("AuditWorker subscribed to events and running");

        // Keep the worker alive
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("AuditWorker shutting down");
    }

    private async Task AuditClaimEscalatedAsync(ClaimEscalatedEvent eventData, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "?? AUDIT: Claim escalation recorded\n" +
            "   ClaimId: {ClaimId}\n" +
            "   Type: {ClaimType}\n" +
            "   Amount: {Amount:C}\n" +
            "   Address: {Address}\n" +
            "   Timestamp: {OccurredAt}",
            eventData.ClaimId,
            eventData.ClaimType,
            eventData.Amount,
            eventData.Address,
            eventData.OccurredAt
        );

        var auditEntry = new AuditEntry
        {
            EventType = nameof(ClaimEscalatedEvent),
            ClaimId = eventData.ClaimId,
            ClaimType = eventData.ClaimType,
            Action = "Escalated",
            NewValue = $"Amount: {eventData.Amount:C}",
            Timestamp = eventData.OccurredAt,
            Metadata = new Dictionary<string, object>
            {
                ["Amount"] = eventData.Amount,
                ["Address"] = eventData.Address
            }
        };

        await _auditRepository.SaveAsync(auditEntry, cancellationToken);
    }

    private async Task AuditClaimFraudFlaggedAsync(ClaimFraudFlaggedEvent eventData, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "?? AUDIT: Fraud flag recorded\n" +
            "   ClaimId: {ClaimId}\n" +
            "   Type: {ClaimType}\n" +
            "   Address: {Address}\n" +
            "   Reason: {FraudReason}\n" +
            "   Timestamp: {OccurredAt}",
            eventData.ClaimId,
            eventData.ClaimType,
            eventData.Address,
            eventData.FraudReason,
            eventData.OccurredAt
        );

        var auditEntry = new AuditEntry
        {
            EventType = nameof(ClaimFraudFlaggedEvent),
            ClaimId = eventData.ClaimId,
            ClaimType = eventData.ClaimType,
            Action = "FraudFlagged",
            NewValue = eventData.FraudReason,
            Timestamp = eventData.OccurredAt,
            Metadata = new Dictionary<string, object>
            {
                ["Address"] = eventData.Address,
                ["FraudReason"] = eventData.FraudReason
            }
        };

        await _auditRepository.SaveAsync(auditEntry, cancellationToken);
    }

    private async Task AuditClaimStatusChangedAsync(ClaimStatusChangedEvent eventData, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "?? AUDIT: Status change recorded\n" +
            "   ClaimId: {ClaimId}\n" +
            "   Type: {ClaimType}\n" +
            "   OldStatus: {OldStatus}\n" +
            "   NewStatus: {NewStatus}\n" +
            "   Timestamp: {OccurredAt}",
            eventData.ClaimId,
            eventData.ClaimType,
            eventData.OldStatus,
            eventData.NewStatus,
            eventData.OccurredAt
        );

        var auditEntry = new AuditEntry
        {
            EventType = nameof(ClaimStatusChangedEvent),
            ClaimId = eventData.ClaimId,
            ClaimType = eventData.ClaimType,
            Action = "StatusChanged",
            OldValue = eventData.OldStatus,
            NewValue = eventData.NewStatus,
            Timestamp = eventData.OccurredAt
        };

        await _auditRepository.SaveAsync(auditEntry, cancellationToken);
    }
}
