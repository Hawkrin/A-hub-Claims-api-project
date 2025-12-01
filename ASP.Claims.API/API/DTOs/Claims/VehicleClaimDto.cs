using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.API.DTOs.Claims;

public record VehicleClaimDto
{
    public Guid Id { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public string PlaceOfAccident { get; set; } = string.Empty;

    public DateTime ReportedDate { get; set; } = DateTime.UtcNow;

    public string Description { get; set; } = string.Empty;

    public ClaimStatus Status { get; set; } = ClaimStatus.None;
}