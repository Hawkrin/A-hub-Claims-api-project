using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using AutoMapper;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdateTravelClaimHandler(IClaimRepository repository, IMapper mapper) : IRequestHandler<UpdateTravelClaimCommand, Result<TravelClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<TravelClaim>> Handle(UpdateTravelClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);
        if (existingClaim is not TravelClaim travelClaim)
            return Result.Fail<TravelClaim>(ErrorMessages.ErrorMessage_ClaimNotFound);

        _mapper.Map(command, travelClaim);

        var updateResult = await _repository.UpdateClaim(travelClaim);
        if (updateResult.IsFailed)
            return Result.Fail<TravelClaim>(updateResult.Errors[0].Message);

        return Result.Ok(travelClaim);
    }
}
