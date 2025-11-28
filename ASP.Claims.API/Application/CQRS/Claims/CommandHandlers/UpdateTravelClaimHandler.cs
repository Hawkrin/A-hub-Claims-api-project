using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using AutoMapper;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdateTravelClaimHandler(IClaimRepository repository, IMapper mapper) : IRequestHandler<UpdateTravelClaimCommand, Result>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result> Handle(UpdateTravelClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);
        if (existingClaim is not TravelClaim)
            return Result.Fail(ErrorMessages.ErrorMessage_ClaimNotFound);

        var claim = _mapper.Map<TravelClaim>(command);

        return await _repository.UpdateClaim(claim);  
    }
}
