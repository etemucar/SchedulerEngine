using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class DeletePartyRoleCommandHandler : IRequestHandler<DeletePartyRoleCommand, bool>
{
    private readonly IRepository<PartyRole, int> _partyRoleRepository;
    private readonly ILogger<DeletePartyRoleCommandHandler> _logger;

    public DeletePartyRoleCommandHandler(
        IRepository<PartyRole, int> partyRoleRepository,
        ILogger<DeletePartyRoleCommandHandler> logger)
    {
        _partyRoleRepository = partyRoleRepository;
        _logger              = logger;
    }

    public async Task<bool> Handle(DeletePartyRoleCommand request, CancellationToken cancellationToken)
    {
        var partyRole = await _partyRoleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (partyRole is null)
            return false;

        await _partyRoleRepository.RemoveAsync(partyRole, cancellationToken);

        _logger.LogInformation("PartyRole silindi. PartyRoleId: {PartyRoleId}", partyRole.Id);

        return true;
    }
}
