using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class PatchOrganizationCommandHandler : IRequestHandler<PatchOrganizationCommand, OrganizationResponse>
{
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly ILogger<PatchOrganizationCommandHandler> _logger;

    public PatchOrganizationCommandHandler(
        IRepository<Organization, int> organizationRepository,
        ILogger<PatchOrganizationCommandHandler> logger)
    {
        _organizationRepository = organizationRepository;
        _logger                 = logger;
    }

    public async Task<OrganizationResponse> Handle(PatchOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (organization is null)
            return null!;

        // Sadece gelen alanları güncelle (merge-patch)
        if (request.Name               is not null) organization.Name               = request.Name;
        if (request.TaxOffice          is not null) organization.TaxOffice          = request.TaxOffice;
        if (request.TaxNumber          is not null) organization.TaxNumber          = request.TaxNumber.Value;
        if (request.IdentityNumber     is not null) organization.IdentityNumber     = request.IdentityNumber.Value;
        if (request.TradeName          is not null) organization.TradeName          = request.TradeName;
        if (request.TradeRegisterNumber is not null) organization.TradeRegisterNumber = request.TradeRegisterNumber.Value;
        if (request.MersisNo           is not null) organization.MersisNo           = request.MersisNo.Value;

        organization.ValidForStart = request.ValidForStart ?? organization.ValidForStart;
        organization.ValidForEnd   = request.ValidForEnd   ?? organization.ValidForEnd;        

        await _organizationRepository.UpdateAsync(organization, cancellationToken);

        _logger.LogInformation("Organization güncellendi. OrganizationId: {OrganizationId}", organization.Id);

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
