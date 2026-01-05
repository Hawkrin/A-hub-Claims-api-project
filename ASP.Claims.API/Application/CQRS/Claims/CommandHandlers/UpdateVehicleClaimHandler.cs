using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using AutoMapper;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdateVehicleClaimHandler(IClaimRepository repository, IMapper mapper, IClaimStatusEvaluator claimStatusEvaluator) : 
    IRequestHandler<UpdateVehicleClaimCommand, Result<VehicleClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<VehicleClaim>> Handle(UpdateVehicleClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);

        if (existingClaim is not VehicleClaim vehicleClaim)
            return Result.Fail<VehicleClaim>(ErrorMessages.ErrorMessage_ClaimNotFound);

        // Map updates onto the existing entity
        _mapper.Map(command, vehicleClaim);
        vehicleClaim.Status = _claimStatusEvaluator.Evaluate(vehicleClaim, null);

        var updateResult = await _repository.UpdateClaim(vehicleClaim);
        if (updateResult.IsFailed)
            return Result.Fail<VehicleClaim>(updateResult.Errors[0].Message);

        return Result.Ok(vehicleClaim);
    }
}