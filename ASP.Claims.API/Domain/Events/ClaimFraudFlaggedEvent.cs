// Re-export shared event from ServiceDefaults
namespace ASP.Claims.API.Domain.Events;

public record ClaimFraudFlaggedEvent(
    Guid ClaimId,
    string ClaimType,
    string Address,
    string FraudReason,
    DateTime OccurredAt
) : ASP.Claims.ServiceDefaults.Events.ClaimFraudFlaggedEvent(ClaimId, ClaimType, Address, FraudReason, OccurredAt);
