using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.Domain.Entities;

public abstract class Claim
{
    public Guid Id { get; set; }

    public ClaimType Type { get; set; }

    public DateTime ReportedDate { get; set; } = DateTime.UtcNow;

    public string Description { get; set; } = string.Empty;

    public ClaimStatus? Status { get; set; }

}