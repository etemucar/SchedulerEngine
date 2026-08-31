using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.TMFCommon;

namespace SchedulerEngine.Service.Features.Handlers;

public class CreatePartyRoleCommandHandler : IRequestHandler<CreatePartyRoleCommand, PartyRoleResponse>
{
    private readonly IRepository<PartyRole, int>                   _partyRoleRepository;
    private readonly ILogger<CreatePartyRoleCommandHandler>        _logger;

    public CreatePartyRoleCommandHandler(
        IRepository<PartyRole, int>               partyRoleRepository,
        ILogger<CreatePartyRoleCommandHandler>    logger)
    {
        _partyRoleRepository = partyRoleRepository;
        _logger              = logger;
    }

    public async Task<PartyRoleResponse> Handle(CreatePartyRoleCommand request, CancellationToken cancellationToken)
    {
        var partyRole = new PartyRole
        {
            PartyId         = request.PartyId,
            PartyRoleTypeId = request.PartyRoleTypeId,
            // null gelirse Min/Max ile aç — kural 4
            ValidForStart   = request.ValidForStart ?? DateTime.MinValue,
            ValidForEnd     = request.ValidForEnd   ?? DateTime.MaxValue,
        };

        await _partyRoleRepository.AddAsync(partyRole, cancellationToken);

        _logger.LogInformation(
            "PartyRole oluşturuldu. PartyId: {PartyId}, PartyRoleTypeCd: {PartyRoleTypeCd}, PartyRoleId: {PartyRoleId}",
            partyRole.PartyId, partyRole.PartyRoleTypeId, partyRole.Id);

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
