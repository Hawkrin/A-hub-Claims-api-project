using ASP.Claims.API.API.DTOs.Claims;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Domain.Entities;
using AutoMapper;

namespace ASP.Claims.API.Application.Profiles;

public class VehicleClaimMappingProfile : Profile
{
    public VehicleClaimMappingProfile() 
    {
        CreateMap<VehicleClaimDto, CreateVehicleClaimCommand>();

        CreateMap<VehicleClaimDto, UpdateVehicleClaimCommand>();

        CreateMap<CreateVehicleClaimCommand, VehicleClaim>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        CreateMap<UpdateVehicleClaimCommand, VehicleClaim>();
    }
}
