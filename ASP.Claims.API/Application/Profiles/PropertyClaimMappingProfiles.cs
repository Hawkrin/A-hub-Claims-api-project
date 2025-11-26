using AutoMapper;
using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Domain.Entities;

namespace ASP.Claims.API.Application.Profiles;

public class PropertyClaimMappingProfiles : Profile
{
    public PropertyClaimMappingProfiles()
    {
        // PropertyClaim
        CreateMap<PropertyClaimDto, CreatePropertyClaimCommand>();
        CreateMap<PropertyClaimDto, UpdatePropertyClaimCommand>();
        CreateMap<CreatePropertyClaimCommand, PropertyClaim>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.Ignore()); // Status set by evaluator
        CreateMap<UpdatePropertyClaimCommand, PropertyClaim>();

        // VehicleClaim
        CreateMap<VehicleClaimDto, CreateVehicleClaimCommand>();
        CreateMap<VehicleClaimDto, UpdateVehicleClaimCommand>();
        CreateMap<CreateVehicleClaimCommand, VehicleClaim>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.Ignore());
        CreateMap<UpdateVehicleClaimCommand, VehicleClaim>();
    }
}
