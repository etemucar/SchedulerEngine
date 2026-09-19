using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Queries;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class GetOrganizationQueryHandler : IRequestHandler<GetOrganizationQuery, OrganizationResponse>
{
    private readonly IRepository<Organization, int> _organizationRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrganizationQueryHandler> _logger;

    public GetOrganizationQueryHandler(
        IRepository<Organization, int> organizationRepository,
        IMapper mapper,
        ILogger<GetOrganizationQueryHandler> logger)
    {
        _organizationRepository = organizationRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrganizationResponse> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (organization is null)
            throw new NotFoundException($"Organization bulunamadı: {request.Id}");

        return _mapper.Map<OrganizationResponse>(organization);
    }
}