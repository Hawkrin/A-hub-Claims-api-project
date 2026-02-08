using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.Application.Interfaces;

/// <summary>
/// Helper service to publish domain events for claims
/// Encapsulates event publishing logic and handles failures gracefully
/// </summary>
public interface IClaimEventPublisher
{
    /// <summary>
    /// Publishes all relevant events for a claim based on its status
    /// </summary>
    Task PublishClaimEventsAsync(Claim claim, CancellationToken cancellationToken = default);
}
