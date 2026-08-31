using MediatR;
using SchedulerEngine.Service.Dtos.Responses;

namespace SchedulerEngine.Service.Features.Queries;

public record GetCurrentUserQuery(int UserId) : IRequest<CurrentUserResponse>;
