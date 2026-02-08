namespace ASP.Claims.ServiceDefaults.Events;

public record ClaimStatusChangedEvent(
    Guid ClaimId,
    string ClaimType,
    string OldStatus,
    string NewStatus,
    DateTime OccurredAt
);
