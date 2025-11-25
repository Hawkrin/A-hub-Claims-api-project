namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using FluentResults;
using MediatR;

public class CreateTravelClaimHandler(IClaimRepository repository) : IRequestHandler<CreateTravelClaimCommand, Result<Guid>>
{
    private readonly IClaimRepository _repository = repository;

    public async Task<Result<Guid>> Handle(CreateTravelClaimCommand command, CancellationToken cancellationToken)
    {
        var claim = new TravelClaim
        {
            Id = Guid.NewGuid(),
            Country = command.Country,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            IncidentType = command.IncidentType,
            Description = command.Description,
            ReportedDate = command.ReportedDate,
            Status = ClaimStatus.None,
        };

        var saveResult = await _repository.Save(claim);
        if (saveResult.IsFailed)
            return Result.Fail<Guid>(saveResult.Errors[0].Message);

        return Result.Ok(claim.Id);
    }
}
