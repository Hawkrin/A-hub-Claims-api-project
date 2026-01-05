using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using AutoMapper;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdatePropertyClaimHandler(IClaimRepository repository, IMapper mapper, IClaimStatusEvaluator claimStatusEvaluator) :
    IRequestHandler<UpdatePropertyClaimCommand, Result<PropertyClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PropertyClaim>> Handle(UpdatePropertyClaimCommand command, CancellationToken cancellationToken)
    {
        var claim = await _repository.GetById(command.Id);
        if (claim == null)
            return Result.Fail<PropertyClaim>(ErrorMessages.ErrorMessage_ClaimNotFound);

        _mapper.Map(command, claim);

        var allClaims = await _repository.GetByType(claim.Type);
        claim.Status = _claimStatusEvaluator.Evaluate(claim, allClaims);

        var updateResult = await _repository.UpdateClaim(claim);
        if (updateResult.IsFailed)
            return Result.Fail<PropertyClaim>(updateResult.Errors[0].Message);

        if (claim is not PropertyClaim propertyClaim)
            return Result.Fail<PropertyClaim>("Claim is not a PropertyClaim.");

        return Result.Ok(propertyClaim);
    }
}