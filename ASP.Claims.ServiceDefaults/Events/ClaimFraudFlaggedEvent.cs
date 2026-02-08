namespace ASP.Claims.ServiceDefaults.Events;

public record ClaimFraudFlaggedEvent(
    Guid ClaimId,
    string ClaimType,
    string Address,
    string FraudReason,
    DateTime OccurredAt
);
