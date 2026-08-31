using AutoMapper;
using SchedulerEngine.Api.Models;
using SchedulerEngine.Api.Models.TMFOpenApi5;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Service.Dtos.Requests;
using SchedulerEngine.Service.Dtos.Responses;

namespace SchedulerEngine.Api.Mapping;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        // ── Request mappings (Model → Command) ──────────────────────────

        CreateMap<TimePeriodModel, TimePeriodRequest>();

        CreateMap<ContactMediumModel, ContactMediumRequest>()
            .ForMember(dest => dest.Characteristic, opt => opt.MapFrom(src =>
                src.Characteristic != null
                    ? ObjectToDictionary(src.Characteristic)
                    : new Dictionary<string, object>()));

        CreateMap<RelatedPartyModel, RelatedPartyRequest>()
            .ForMember(dest => dest.PartyOrPartyRole, opt => opt.MapFrom(src => src.PartyOrPartyRole));

        CreateMap<PartyOrPartyRoleModel, PartyOrPartyRoleRequest>();

        CreateMap<IndividualModel, CreateIndividualCommand>()
            .ForMember(dest => dest.ContactMedium, opt => opt.MapFrom(src => src.ContactMedium))
            .ForMember(dest => dest.RelatedParty,  opt => opt.MapFrom(src => src.RelatedParty));

        CreateMap<IndividualModel, PatchIndividualCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Controller'da set edilir


        // ── Response mappings (Response → Model) ────────────────────────

        CreateMap<TimePeriodResponse, TimePeriodModel>();

        CreateMap<IndividualResponse, IndividualModel>()
            .ForMember(dest => dest.Id,       opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Href,     opt => opt.Ignore()) // Controller'da set edilir
            .ForMember(dest => dest.Type,     opt => opt.MapFrom(_ => "Individual"))
            .ForMember(dest => dest.BaseType, opt => opt.MapFrom(_ => "Party"));

        // Request mappings
        CreateMap<CredentialCharacteristicModel, CredentialCharacteristicRequest>();

        CreateMap<CredentialModel, CredentialRequest>()
            .ForMember(dest => dest.ContactMedia, opt => opt.MapFrom(src => src.ContactMedia));

        CreateMap<DigitalIdentityModel, CreateDigitalIdentityCommand>()
            .ForMember(dest => dest.Credentials, opt => opt.MapFrom(src => src.Credentials));

        // ── B7: DigitalIdentity Patch mapping'i (FinYo'dan taşındı) ─────
        CreateMap<CredentialModel, CredentialPatchRequest>()
            .ForMember(dest => dest.ContactMedia, opt => opt.MapFrom(src => src.ContactMedia));

        CreateMap<DigitalIdentityModel, PatchDigitalIdentityCommand>()
            .ForMember(dest => dest.DigitalIdentityId, opt => opt.Ignore()) // Controller'da route'tan set edilir
            .ForMember(dest => dest.Credentials, opt => opt.MapFrom(src => src.Credentials));

        // Response mappings
        CreateMap<DigitalIdentityResponse, DigitalIdentityModel>()
            .ForMember(dest => dest.Id,     opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Href,   opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // ── Scheduler / Jobs mappings (TMF dışı - Auth/Admin ile aynı konvansiyon) ─────

        CreateMap<ExternalTaskRequestModel, EnqueueExternalTaskCommand>();
        CreateMap<ScheduleExternalTaskRequestModel, ScheduleExternalTaskCommand>();
        CreateMap<RecurringJobRequestModel, AddOrUpdateRecurringJobCommand>();

        CreateMap<EnqueueJobResult, EnqueueJobResponse>()
            .ForMember(dest => dest.Success, opt => opt.Ignore()); // varsayılan (true) korunur

        CreateMap<ScheduleJobResult, ScheduleJobResponse>()
            .ForMember(dest => dest.Success, opt => opt.Ignore());

        CreateMap<RecurringJobResult, RecurringJobResponse>()
            .ForMember(dest => dest.Success, opt => opt.Ignore());

        CreateMap<RecurringJobListItem, RecurringJobListItemResponse>();

        // ── Organization / Servis kaydı (TMF dışı - Auth'ta register-organization) ─────
        // Credentials (List<CredentialModel> -> List<CredentialRequest>) yukarıdaki
        // CreateMap<CredentialModel, CredentialRequest>() ile otomatik çözülüyor,
        // ekstra bir şey yazmaya gerek yok.
        CreateMap<RegisterOrganizationModel, RegisterOrganizationCommand>();

    }

    private static Dictionary<string, object> ObjectToDictionary(object obj)
    {
        var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (obj == null) return dictionary;

        foreach (var prop in obj.GetType().GetProperties())
        {
            var value = prop.GetValue(obj);
            if (value != null)
                dictionary[char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..]] = value;
        }

        return dictionary;
    }
}