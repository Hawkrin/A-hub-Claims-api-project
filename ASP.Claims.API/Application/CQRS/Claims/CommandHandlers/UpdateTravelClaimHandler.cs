using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdateTravelClaimHandler(IClaimRepository repository) : IRequestHandler<UpdateTravelClaimCommand, Result>
{
    private readonly IClaimRepository _repository = repository;

    public async Task<Result> Handle(UpdateTravelClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);

        if (existingClaim is not TravelClaim claim)
            return Result.Fail(ErrorMessages.ErrorMessage_ClaimNotFound);

        claim.StartDate = command.StartDate;
        claim.EndDate = command.EndDate;
        claim.Description = command.Description;
        claim.Country = command.Country;
        claim.Status = command.Status;
        claim.ReportedDate = command.ReportedDate;

        return await _repository.UpdateClaim(claim);  
    }
}
