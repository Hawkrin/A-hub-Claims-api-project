using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.API.DTOs;

public record PropertyClaimDto
{
    public Guid Id { get; set; }

    public string Address { get; set; } = string.Empty;

    public PropertyDamageType PropertyDamageType { get; set; }

    public decimal EstimatedDamageCost { get; set; }

    public DateTime ReportedDate { get; set; } = DateTime.UtcNow;

    public string Description { get; set; } = string.Empty;

    public ClaimStatus Status { get; set; } = ClaimStatus.None;
}