// Re-export shared event from ServiceDefaults
namespace ASP.Claims.API.Domain.Events;

public record ClaimEscalatedEvent(
    Guid ClaimId,
    string ClaimType,
    decimal Amount,
    string Address,
    DateTime OccurredAt
) : ASP.Claims.ServiceDefaults.Events.ClaimEscalatedEvent(ClaimId, ClaimType, Amount, Address, OccurredAt);
