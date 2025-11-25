using ASP.Claims.API.Application.CQRS.Claims.Queries;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.QueryHandlers;

public class ClaimQueryHandler(IClaimRepository repository) :
    IRequestHandler<GetAllClaimsQuery, IEnumerable<Claim>>
{
    private readonly IClaimRepository _repository = repository;

    public async Task<IEnumerable<Claim>> Handle(GetAllClaimsQuery request, CancellationToken cancellationToken)
        => await _repository.GetAll();
}
