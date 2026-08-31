using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.TMFCommon;

namespace SchedulerEngine.Service.Features.Handlers;

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, OrganizationResponse>
{
    private readonly IRepository<Party, int>                          _partyRepository;
    private readonly IRepository<Organization, int>                   _organizationRepository;
    private readonly ILogger<CreateOrganizationCommandHandler>        _logger;

    public CreateOrganizationCommandHandler(
        IRepository<Party, int>                    partyRepository,
        IRepository<Organization, int>             organizationRepository,
        ILogger<CreateOrganizationCommandHandler>  logger)
    {
        _partyRepository        = partyRepository;
        _organizationRepository = organizationRepository;
        _logger                 = logger;
    }

    public async Task<OrganizationResponse> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        // 1. Party oluştur (abstract container)
        var party = new Party();
        await _partyRepository.AddAsync(party, cancellationToken);

        // 2. Organization oluştur
        var organization = new Organization
        {
            PartyId             = party.Id,
            Name                = request.Name,
            TaxOffice           = request.TaxOffice,
            TaxNumber           = request.TaxNumber,
            IdentityNumber      = request.IdentityNumber,
            TradeName           = request.TradeName,
            TradeRegisterNumber = request.TradeRegisterNumber,
            MersisNo            = request.MersisNo,
            // null gelirse Min/Max ile aç — kural 4
            ValidForStart       = request.ValidForStart ?? DateTime.MinValue,
            ValidForEnd         = request.ValidForEnd   ?? DateTime.MaxValue,
        };

        await _organizationRepository.AddAsync(organization, cancellationToken);

        _logger.LogInformation(
            "Organization oluşturuldu. PartyId: {PartyId}, OrganizationId: {OrganizationId}",
            party.Id, organization.Id);

        return MapToResponse(organization);
    }

    private static OrganizationResponse MapToResponse(Organization organization) => new()
    {
        Id                  = organization.Id,
        Name                = organization.Name,
        TaxOffice           = organization.TaxOffice,
        TaxNumber           = organization.TaxNumber,
        IdentityNumber      = organization.IdentityNumber,
        TradeName           = organization.TradeName,
        TradeRegisterNumber = organization.TradeRegisterNumber,
        MersisNo            = organization.MersisNo,
        ValidFor = new TimePeriodResponse
        {
            StartDateTime = organization.ValidForStart,
            EndDateTime   = organization.ValidForEnd
        }
    };
}
