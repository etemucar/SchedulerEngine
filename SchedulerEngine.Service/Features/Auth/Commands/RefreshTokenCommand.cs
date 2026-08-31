using MediatR;
using SchedulerEngine.Service.Dtos.Requests;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Core.Common.Behaviors;

namespace SchedulerEngine.Service.Features.Commands;

public class RefreshTokenCommand : IRequest<AuthResult>, ITransactionalRequest
{
    public string RefreshToken { get; set; } = null!;
}