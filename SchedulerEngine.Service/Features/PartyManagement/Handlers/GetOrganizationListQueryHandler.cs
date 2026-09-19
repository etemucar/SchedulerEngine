// GetOrganizationListQueryHandler.cs
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Queries;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class GetOrganizationListQueryHandler : IRequestHandler<GetOrganizationListQuery, IEnumerable<OrganizationResponse>>
{
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrganizationListQueryHandler> _logger;

    public GetOrganizationListQueryHandler(
        IRepository<Organization, int> organizationRepository,
        IMapper mapper,
        ILogger<GetOrganizationListQueryHandler> logger)
    {
        _organizationRepository = organizationRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<OrganizationResponse>> Handle(GetOrganizationListQuery request, CancellationToken cancellationToken)
    {
        var organizations = await _organizationRepository.FindAsync(
            predicate: _ => true,
            orderBy: q => q.OrderBy(x => x.Id),
            ct: cancellationToken);

        return organizations.Select(_mapper.Map<OrganizationResponse>);
    }
}