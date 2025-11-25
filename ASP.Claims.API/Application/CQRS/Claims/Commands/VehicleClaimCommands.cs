using ASP.Claims.API.Domain.Enums;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.Commands;

public record CreateVehicleClaimCommand(
    string RegistrationNumber,
    string PlaceOfAccident,
    DateTime ReportedDate,
    string Description
) : IRequest<Result<Guid>>;

public record UpdateVehicleClaimCommand(
    Guid Id,
    string RegistrationNumber,
    string PlaceOfAccident,
    DateTime ReportedDate,
    string Description,
    ClaimStatus? Status
) : IRequest<Result>;

public record DeleteVehicleClaimCommand(Guid Id) : IRequest<Result>;