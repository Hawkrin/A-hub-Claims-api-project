using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Domain.Entities;
using AutoMapper;

namespace ASP.Claims.API.Application.Profiles;

public class TravelClaimMappingProfile : Profile
{
    public TravelClaimMappingProfile()
    {
        CreateMap<TravelClaimDto, CreateTravelClaimCommand>();

        CreateMap<TravelClaimDto, UpdateTravelClaimCommand>();

        CreateMap<CreateTravelClaimCommand, TravelClaim>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        CreateMap<UpdateTravelClaimCommand, TravelClaim>();
    }
}
