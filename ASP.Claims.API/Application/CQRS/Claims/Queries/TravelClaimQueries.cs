using MediatR;
using ASP.Claims.API.Domain.Entities;

namespace ASP.Claims.API.Application.CQRS.Claims.Queries;

public record GetTravelClaimByIdQuery(Guid Id) : IRequest<TravelClaim?>;
public record GetAllTravelClaimsQuery() : IRequest<IEnumerable<TravelClaim>>;