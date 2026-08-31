using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Queries;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class GetOrganizationListQueryHandler : IRequestHandler<GetOrganizationListQuery, IEnumerable<OrganizationResponse>>
{
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly ILogger<GetOrganizationListQueryHandler> _logger;

    public GetOrganizationListQueryHandler(
        IRepository<Organization, int> organizationRepository,
        ILogger<GetOrganizationListQueryHandler> logger)
    {
        _organizationRepository = organizationRepository;
        _logger                 = logger;
    }

    public async Task<IEnumerable<OrganizationResponse>> Handle(GetOrganizationListQuery request, CancellationToken cancellationToken)
    {
        var organizations = await _organizationRepository.FindAsync(
            predicate: _ => true,
            orderBy:   q => q.OrderBy(x => x.Id),
            ct:        cancellationToken);

        return organizations.Select(MapToResponse);
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
