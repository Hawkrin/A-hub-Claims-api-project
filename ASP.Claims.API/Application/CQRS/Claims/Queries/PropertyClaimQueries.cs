using ASP.Claims.API.Domain.Entities;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.Queries;

public record GetPropertyClaimByIdQuery(Guid Id) : IRequest<PropertyClaim?>;
public record GetAllPropertyClaimsQuery() : IRequest<IEnumerable<PropertyClaim>>;
