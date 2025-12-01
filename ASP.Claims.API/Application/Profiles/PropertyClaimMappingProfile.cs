using AutoMapper;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.API.DTOs.Claims;

namespace ASP.Claims.API.Application.Profiles;

public class PropertyClaimMappingProfile : Profile
{
    public PropertyClaimMappingProfile()
    {
        CreateMap<PropertyClaimDto, CreatePropertyClaimCommand>();

        CreateMap<PropertyClaimDto, UpdatePropertyClaimCommand>();

        CreateMap<CreatePropertyClaimCommand, PropertyClaim>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.Ignore()); // Status set by evaluator

        CreateMap<UpdatePropertyClaimCommand, PropertyClaim>();
    }
}
