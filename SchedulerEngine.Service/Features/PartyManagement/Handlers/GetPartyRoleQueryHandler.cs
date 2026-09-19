using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Queries;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class GetPartyRoleQueryHandler : IRequestHandler<GetPartyRoleQuery, PartyRoleResponse>
{
    private readonly IRepository<PartyRole, int> _partyRoleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPartyRoleQueryHandler> _logger;

    public GetPartyRoleQueryHandler(
        IRepository<PartyRole, int> partyRoleRepository,
        IMapper mapper,
        ILogger<GetPartyRoleQueryHandler> logger)
    {
        _partyRoleRepository = partyRoleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PartyRoleResponse> Handle(GetPartyRoleQuery request, CancellationToken cancellationToken)
    {
        var partyRole = await _partyRoleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (partyRole is null)
            throw new NotFoundException($"PartyRole bulunamadı: {request.Id}");

        return _mapper.Map<PartyRoleResponse>(partyRole);
    }
}