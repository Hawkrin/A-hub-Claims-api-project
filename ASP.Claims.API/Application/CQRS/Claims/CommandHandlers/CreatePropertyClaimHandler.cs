namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using FluentResults;
using MediatR;

public class CreatePropertyClaimHandler(IClaimRepository repository, IClaimStatusEvaluator claimStatusEvaluator) : 
    IRequestHandler<CreatePropertyClaimCommand, Result<Guid>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;

    public async Task<Result<Guid>> Handle(CreatePropertyClaimCommand command, CancellationToken cancellationToken)
    {
        var claim = new PropertyClaim
        {
            Id = Guid.NewGuid(),
            Address = command.Address,
            EstimatedDamageCost = command.EstimatedDamageCost,
            ReportedDate = command.ReportedDate,
            Description = command.Description,
            Status = ClaimStatus.None,
        };

        var allClaims = await _repository.GetByType(claim.Type);
        claim.Status = _claimStatusEvaluator.Evaluate(claim, allClaims);

        var saveResult = await _repository.Save(claim);

        if (saveResult.IsFailed)
            return Result.Fail<Guid>(saveResult.Errors[0].Message);

        return Result.Ok(claim.Id);
    }
}
