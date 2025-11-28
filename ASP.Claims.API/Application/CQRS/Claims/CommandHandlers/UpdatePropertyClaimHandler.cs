using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using AutoMapper;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdatePropertyClaimHandler(IClaimRepository repository, IMapper mapper, IClaimStatusEvaluator claimStatusEvaluator) : 
    IRequestHandler<UpdatePropertyClaimCommand, Result>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;
    private readonly IMapper _mapper = mapper;

    public async Task<Result> Handle(UpdatePropertyClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);
        if (existingClaim is not PropertyClaim)
            return Result.Fail(ErrorMessages.ErrorMessage_ClaimNotFound);

        var claim = _mapper.Map<PropertyClaim>(command);

        var allClaims = await _repository.GetByType(claim.Type);
        claim.Status = _claimStatusEvaluator.Evaluate(claim, allClaims);

        return await _repository.UpdateClaim(claim);
    }
}