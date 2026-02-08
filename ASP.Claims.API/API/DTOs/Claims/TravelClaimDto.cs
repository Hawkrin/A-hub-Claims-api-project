using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.API.DTOs.Claims;

public record TravelClaimDto
{
    public Guid? Id { get; set; }

    public Country Country { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public IncidentType IncidentType { get; set; }

    public DateTime ReportedDate { get; set; } = DateTime.UtcNow;

    public string Description { get; set; } = string.Empty;

    public ClaimStatus? Status { get; set; }
}

