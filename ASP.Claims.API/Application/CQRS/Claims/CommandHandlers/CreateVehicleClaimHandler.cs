namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using FluentResults;
using MediatR;

public class CreateVehicleClaimHandler(IClaimRepository repository, IClaimStatusEvaluator claimStatusEvaluator) : 
    IRequestHandler<CreateVehicleClaimCommand, Result<Guid>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;

    public async Task<Result<Guid>> Handle(CreateVehicleClaimCommand command, CancellationToken cancellationToken)
    {
        var claim = new VehicleClaim
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = command.RegistrationNumber,
            PlaceOfAccident = command.PlaceOfAccident,
            Description = command.Description,
            ReportedDate = command.ReportedDate,
            Status = ClaimStatus.None,
        };

        claim.Status = _claimStatusEvaluator.Evaluate(claim, null);

        var saveResult = await _repository.Save(claim);
        if (saveResult.IsFailed)
            return Result.Fail<Guid>(saveResult.Errors[0].Message);

        return Result.Ok(claim.Id);
    }
}