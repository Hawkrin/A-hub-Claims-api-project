using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using AutoMapper;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdateVehicleClaimHandler(IClaimRepository repository, IMapper mapper, IClaimStatusEvaluator claimStatusEvaluator) : 
    IRequestHandler<UpdateVehicleClaimCommand, Result>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;
    private readonly IMapper _mapper = mapper;

    public async Task<Result> Handle(UpdateVehicleClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);

        if (existingClaim is not VehicleClaim)
            return Result.Fail(ErrorMessages.ErrorMessage_ClaimNotFound);

        var claim = _mapper.Map<VehicleClaim>(command);
        claim.Status = _claimStatusEvaluator.Evaluate(claim, null);

        return await _repository.UpdateClaim(claim);
    }
}