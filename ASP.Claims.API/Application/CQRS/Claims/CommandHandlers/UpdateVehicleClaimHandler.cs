using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdateVehicleClaimHandler(IClaimRepository repository, IClaimStatusEvaluator claimStatusEvaluator) : 
    IRequestHandler<UpdateVehicleClaimCommand, Result>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;

    public async Task<Result> Handle(UpdateVehicleClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);

        if (existingClaim is not VehicleClaim claim)
            return Result.Fail(ErrorMessages.ErrorMessage_ClaimNotFound);

        claim.PlaceOfAccident = command.PlaceOfAccident;
        claim.RegistrationNumber = command.RegistrationNumber;
        claim.Description = command.Description;
        claim.Status = command.Status ?? null;
        claim.ReportedDate = command.ReportedDate;

        claim.Status = _claimStatusEvaluator.Evaluate(claim, null);

        return await _repository.UpdateClaim(claim);
    }
}