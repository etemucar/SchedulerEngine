using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class PatchPartyRoleCommandHandler : IRequestHandler<PatchPartyRoleCommand, PartyRoleResponse>
{
    private readonly IRepository<PartyRole, int> _partyRoleRepository;
    private readonly ILogger<PatchPartyRoleCommandHandler> _logger;

    public PatchPartyRoleCommandHandler(
        IRepository<PartyRole, int> partyRoleRepository,
        ILogger<PatchPartyRoleCommandHandler> logger)
    {
        _partyRoleRepository = partyRoleRepository;
        _logger              = logger;
    }

    public async Task<PartyRoleResponse> Handle(PatchPartyRoleCommand request, CancellationToken cancellationToken)
    {
        var partyRole = await _partyRoleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (partyRole is null)
            return null!;

        if (request.PartyRoleTypeId is not null) 
        {
            partyRole.PartyRoleTypeId = request.PartyRoleTypeId.Value;
        }

        partyRole.ValidForStart = request.ValidForStart ?? partyRole.ValidForStart;
        partyRole.ValidForEnd   = request.ValidForEnd   ?? partyRole.ValidForEnd;

        await _partyRoleRepository.UpdateAsync(partyRole, cancellationToken);

        _logger.LogInformation("PartyRole güncellendi. PartyRoleId: {PartyRoleId}", partyRole.Id);

        return MapToResponse(partyRole);
    }

    private static PartyRoleResponse MapToResponse(PartyRole partyRole) => new()
    {
        Id              = partyRole.Id,
        PartyId         = partyRole.PartyId,
        PartyRoleTypeId = partyRole.PartyRoleTypeId,
        ValidFor = new TimePeriodResponse
        {
            StartDateTime = partyRole.ValidForStart,
            EndDateTime   = partyRole.ValidForEnd
        }
    };
}
