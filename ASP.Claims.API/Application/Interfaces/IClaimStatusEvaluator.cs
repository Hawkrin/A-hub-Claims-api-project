using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.Application.Interfaces;

public interface IClaimStatusEvaluator
{
    ClaimStatus Evaluate(Claim claim, IEnumerable<Claim>? allClaims);
}
