using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class PatchPartyRoleCommandHandler : IRequestHandler<PatchPartyRoleCommand, PartyRoleResponse>
{
    private readonly IRepository<PartyRole, int> _partyRoleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PatchPartyRoleCommandHandler> _logger;

    public PatchPartyRoleCommandHandler(
        IRepository<PartyRole, int> partyRoleRepository,
        IMapper mapper,
        ILogger<PatchPartyRoleCommandHandler> logger)
    {
        _partyRoleRepository = partyRoleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PartyRoleResponse> Handle(PatchPartyRoleCommand request, CancellationToken cancellationToken)
    {
        var partyRole = await _partyRoleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (partyRole is null)
            throw new NotFoundException($"PartyRole bulunamadı: {request.Id}");

        if (request.PartyRoleTypeId is not null)
        {
            partyRole.PartyRoleTypeId = request.PartyRoleTypeId.Value;
        }

        partyRole.ValidForStart = request.ValidForStart ?? partyRole.ValidForStart;
        partyRole.ValidForEnd = request.ValidForEnd ?? partyRole.ValidForEnd;

        await _partyRoleRepository.UpdateAsync(partyRole, cancellationToken);

        _logger.LogInformation("PartyRole güncellendi. PartyRoleId: {PartyRoleId}", partyRole.Id);

        return _mapper.Map<PartyRoleResponse>(partyRole);
    }
}