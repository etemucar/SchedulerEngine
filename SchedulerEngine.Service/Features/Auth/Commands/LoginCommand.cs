using MediatR;
using SchedulerEngine.Service.Dtos.Requests;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Core.Common.Behaviors;

namespace SchedulerEngine.Service.Features.Commands;

public class LoginCommand : IRequest<AuthResult>, ITransactionalRequest
{
    public string Identifier { get; set; } = null!;
    public string Password { get; set; } = null!;
}