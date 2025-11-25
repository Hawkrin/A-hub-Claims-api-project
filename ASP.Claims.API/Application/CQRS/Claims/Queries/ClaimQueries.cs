using ASP.Claims.API.Domain.Entities;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.Queries;

public record GetAllClaimsQuery() : IRequest<IEnumerable<Claim>>;
