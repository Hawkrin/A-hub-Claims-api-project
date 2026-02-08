using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.Domain.Events;

public record ClaimStatusChangedEvent(
    Guid ClaimId,
    string ClaimType,
    string OldStatus,
    string NewStatus,
    DateTime OccurredAt
);
