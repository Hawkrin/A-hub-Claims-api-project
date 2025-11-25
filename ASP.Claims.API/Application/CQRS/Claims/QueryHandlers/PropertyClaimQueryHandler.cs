using ASP.Claims.API.Application.CQRS.Claims.Queries;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.QueryHandlers;

public class PropertyClaimQueryHandler(IClaimRepository repository) : 
    IRequestHandler<GetPropertyClaimByIdQuery, PropertyClaim?>,
    IRequestHandler<GetAllPropertyClaimsQuery, IEnumerable<PropertyClaim>>
{
    private readonly IClaimRepository _repository = repository;
    public async Task<PropertyClaim?> Handle(GetPropertyClaimByIdQuery request, CancellationToken cancellationToken)
        => (await _repository.GetById(request.Id)) as PropertyClaim;

    public async Task<IEnumerable<PropertyClaim>> Handle(GetAllPropertyClaimsQuery request, CancellationToken cancellationToken)
        => (await _repository.GetByType(ClaimType.Property)).OfType<PropertyClaim>();
}