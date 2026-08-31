using MediatR;
using SchedulerEngine.Service.Dtos.Requests;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Core.Common.Behaviors;

namespace SchedulerEngine.Service.Features.Commands;

public class CreatePartyRoleCommand : IRequest<PartyRoleResponse>, ITransactionalRequest
{
    public int    PartyId         { get; set; }
    public int PartyRoleTypeId { get; set; }
    public DateTime? ValidForStart { get; set; }
    public DateTime? ValidForEnd   { get; set; }
}

public class PatchPartyRoleCommand : IRequest<PartyRoleResponse>, ITransactionalRequest
{
    public int     Id              { get; set; }
    public int? PartyRoleTypeId { get; set; }
    public DateTime? ValidForStart { get; set; }
    public DateTime? ValidForEnd   { get; set; }
}
 
public class DeletePartyRoleCommand : IRequest<bool>, ITransactionalRequest
{
    public int Id { get; set; }
}
