using SchedulerEngine.Core.Model;
//using SchedulerEngine.Service.Dtos.Responses;
//using SchedulerEngine.Service.Features.Commands;
using AutoMapper;
using SchedulerEngine.Service.Dtos.Responses;

namespace SchedulerEngine.Service.Mapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── PartyManagement ──────────────────────────────────────────────
        // ValidForStart/ValidForEnd (düz) → ValidFor.StartDateTime/EndDateTime
        // (nested) — convention flatten'ın TERSİ bir dönüşüm olduğu için
        // (unflatten), Watchlist.Tags'teki gibi explicit ForMember şart.
        CreateMap<Individual, IndividualResponse>()
            .ForMember(dest => dest.ValidFor, opt => opt.MapFrom(src => new TimePeriodResponse
            {
                StartDateTime = src.ValidForStart,
                EndDateTime = src.ValidForEnd
            }));

        CreateMap<Organization, OrganizationResponse>()
            .ForMember(dest => dest.ValidFor, opt => opt.MapFrom(src => new TimePeriodResponse
            {
                StartDateTime = src.ValidForStart,
                EndDateTime = src.ValidForEnd
            }));

        CreateMap<PartyRole, PartyRoleResponse>()
            .ForMember(dest => dest.ValidFor, opt => opt.MapFrom(src => new TimePeriodResponse
            {
                StartDateTime = src.ValidForStart,
                EndDateTime = src.ValidForEnd
            }));        

        CreateMap<DigitalIdentity, DigitalIdentityResponse>();               
    }
}
