namespace ASP.Claims.ServiceDefaults.Events;

public record ClaimEscalatedEvent(
    Guid ClaimId,
    string ClaimType,
    decimal Amount,
    string Address,
    DateTime OccurredAt
);
