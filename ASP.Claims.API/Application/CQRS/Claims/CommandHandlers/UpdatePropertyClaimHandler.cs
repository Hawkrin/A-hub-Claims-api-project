using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdatePropertyClaimHandler(IClaimRepository repository, IClaimStatusEvaluator claimStatusEvaluator) : 
    IRequestHandler<UpdatePropertyClaimCommand, Result>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;

    public async Task<Result> Handle(UpdatePropertyClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);

        if (existingClaim is not PropertyClaim claim)
            return Result.Fail(ErrorMessages.ErrorMessage_ClaimNotFound);

        claim.Address = command.Address;
        claim.PropertyDamageType = command.PropertyDamageType;
        claim.EstimatedDamageCost = command.EstimatedDamageCost;
        claim.ReportedDate = command.ReportedDate;
        claim.Description = command.Description;
        claim.Status = command.Status ?? null;

        var allClaims = await _repository.GetByType(claim.Type);
        claim.Status = _claimStatusEvaluator.Evaluate(claim, allClaims);

        return await _repository.UpdateClaim(claim);
    }
}