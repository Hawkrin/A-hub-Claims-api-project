using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using ASP.Claims.ServiceDefaults.Events;

namespace ASP.Claims.API.Application.Services;

/// <summary>
/// Centralized service for publishing claim-related domain events
/// Handles event publishing logic in a DRY manner
/// </summary>
public class ClaimEventPublisher : IClaimEventPublisher
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ClaimEventPublisher> _logger;

    public ClaimEventPublisher(IEventPublisher eventPublisher, ILogger<ClaimEventPublisher> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task PublishClaimEventsAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        if (!claim.Status.HasValue)
        {
            _logger.LogDebug("Claim {ClaimId} has no status, skipping event publishing", claim.Id);
            return;
        }

        var status = claim.Status.Value;

        // Publish events based on claim status flags
        // High-value claim (escalated)
        if (status.HasFlag(ClaimStatus.Escalated))
        {
            await PublishEscalatedEventAsync(claim, cancellationToken);
        }

        // Potential fraud detected
        if (status.HasFlag(ClaimStatus.FraudCheck))
        {
            await PublishFraudEventAsync(claim, cancellationToken);
        }

        // Status changed (for audit trail)
        await PublishStatusChangedEventAsync(claim, cancellationToken);
    }

    private async Task PublishEscalatedEventAsync(Claim claim, CancellationToken cancellationToken)
    {
        try
        {
            // Extract amount and location based on claim type
            var (amount, location) = claim switch
            {
                PropertyClaim property => (property.EstimatedDamageCost, property.Address),
                TravelClaim travel => (0m, travel.Country.ToString()),
                VehicleClaim vehicle => (0m, vehicle.PlaceOfAccident),
                _ => (0m, "Unknown")
            };

            var escalatedEvent = new ClaimEscalatedEvent(
                claim.Id,
                claim.Type.ToString(),
                amount,
                location,
                DateTime.UtcNow
            );

            await _eventPublisher.PublishAsync(escalatedEvent, cancellationToken);
            _logger.LogWarning(
                "High-value claim escalated: ClaimId={ClaimId}, Type={ClaimType}, Amount={Amount:C}, Location={Location}",
                claim.Id, claim.Type, amount, location
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to publish escalated event for claim {ClaimId} (non-critical, continuing)", 
                claim.Id
            );
        }
    }

    private async Task PublishFraudEventAsync(Claim claim, CancellationToken cancellationToken)
    {
        try
        {
            var (location, reason) = claim switch
            {
                PropertyClaim property => (property.Address, "Multiple claims at same address within 6 months"),
                TravelClaim travel => (travel.Country.ToString(), "Suspicious travel claim pattern detected"),
                VehicleClaim vehicle => (vehicle.PlaceOfAccident, "Suspicious vehicle claim pattern detected"),
                _ => ("Unknown", "Fraud pattern detected")
            };

            var fraudEvent = new ClaimFraudFlaggedEvent(
                claim.Id,
                claim.Type.ToString(),
                location,
                reason,
                DateTime.UtcNow
            );

            await _eventPublisher.PublishAsync(fraudEvent, cancellationToken);
            _logger.LogWarning(
                "Potential fraud detected: ClaimId={ClaimId}, Type={ClaimType}, Reason={Reason}",
                claim.Id, claim.Type, reason
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to publish fraud event for claim {ClaimId} (non-critical, continuing)", 
                claim.Id
            );
        }
    }

    private async Task PublishStatusChangedEventAsync(Claim claim, CancellationToken cancellationToken)
    {
        try
        {
            var statusChangedEvent = new ClaimStatusChangedEvent(
                claim.Id,
                claim.Type.ToString(),
                ClaimStatus.None.ToString(),
                claim.Status?.ToString() ?? "None",
                DateTime.UtcNow
            );

            await _eventPublisher.PublishAsync(statusChangedEvent, cancellationToken);
            _logger.LogInformation(
                "Claim status changed: ClaimId={ClaimId}, Type={ClaimType}, NewStatus={Status}",
                claim.Id, claim.Type, claim.Status
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to publish status changed event for claim {ClaimId} (non-critical, continuing)", 
                claim.Id
            );
        }
    }
}
