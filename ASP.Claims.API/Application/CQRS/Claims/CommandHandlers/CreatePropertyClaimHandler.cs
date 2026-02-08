namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

public class CreatePropertyClaimHandler(
    IClaimRepository repository, 
    IMapper mapper, 
    IClaimStatusEvaluator claimStatusEvaluator,
    IClaimEventPublisher eventPublisher,
    ILogger<CreatePropertyClaimHandler> logger) : 
    IRequestHandler<CreatePropertyClaimCommand, Result<PropertyClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;
    private readonly IClaimEventPublisher _eventPublisher = eventPublisher;
    private readonly ILogger<CreatePropertyClaimHandler> _logger = logger;

    public async Task<Result<PropertyClaim>> Handle(CreatePropertyClaimCommand command, CancellationToken cancellationToken)
    {
        var claim = _mapper.Map<PropertyClaim>(command);
        claim.Id = Guid.NewGuid();

        var allClaims = await _repository.GetByType(claim.Type);
        claim.Status = _claimStatusEvaluator.Evaluate(claim, allClaims);

        var saveResult = await _repository.Save(claim);

        if (saveResult.IsFailed)
        {
            _logger.LogError("Failed to create property claim: {Error}", saveResult.Errors[0].Message);
            return Result.Fail<PropertyClaim>(saveResult.Errors[0].Message);
        }

        // Publish domain events (fire-and-forget, non-blocking, non-critical)
        _ = Task.Run(async () => 
        {
            try
            {
                await _eventPublisher.PublishClaimEventsAsync(claim, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event publishing failed for claim {ClaimId} (non-critical)", claim.Id);
            }
        }, cancellationToken);

        _logger.LogInformation(
            "Property claim created: ClaimId={ClaimId}, Amount={Amount:C}, Status={Status}", 
            claim.Id, command.EstimatedDamageCost, claim.Status
        );

        return Result.Ok(claim);
    }
}
