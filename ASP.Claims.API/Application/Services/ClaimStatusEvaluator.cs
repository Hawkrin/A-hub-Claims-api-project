using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.Application.Services;

public class ClaimStatusEvaluator : IClaimStatusEvaluator
{
    public ClaimStatus Evaluate(Claim claim, IEnumerable<Claim>? allClaims = null)
    {
        allClaims ??= [];

        return claim switch
        {
            PropertyClaim propertyClaim => EvaluatePropertyClaim(propertyClaim, allClaims!),
            VehicleClaim vehicleClaim => EvaluateVehicleClaim(vehicleClaim),
            _ => ClaimStatus.None
        };
    }

    private static ClaimStatus EvaluatePropertyClaim(PropertyClaim claim, IEnumerable<Claim> allClaims)
    {
        var status = ClaimStatus.None;

        // BR2: High estimated cost
        if (claim.EstimatedDamageCost > 50000)
            status |= ClaimStatus.Escalated;

        var similarClaims = allClaims
            .Where(c => c is PropertyClaim pc
                && pc.Address == claim.Address
                && c.Id != claim.Id
                && Math.Abs((claim.ReportedDate - pc.ReportedDate).TotalDays) <= 180);

        // BR5: Multiple claims at the same address within 6 months
        if (similarClaims.Any())
            status |= ClaimStatus.FraudCheck;

        return status;
    }

    private static ClaimStatus EvaluateVehicleClaim(VehicleClaim claim)
    {
        var status = ClaimStatus.None;

        // BR1: Claim reported more than 30 days after the incident
        if ((DateTime.UtcNow - claim.ReportedDate).TotalDays > 30)
            status |= ClaimStatus.RequiresManualReview;

        return status;
    }
}