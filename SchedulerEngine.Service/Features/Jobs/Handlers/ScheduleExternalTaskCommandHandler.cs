using Hangfire;
using MediatR;
using SchedulerEngine.Core.Interfaces;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Handlers;

public class ScheduleExternalTaskCommandHandler : IRequestHandler<ScheduleExternalTaskCommand, ScheduleJobResult>
{
    public Task<ScheduleJobResult> Handle(ScheduleExternalTaskCommand request, CancellationToken ct)
    {
        var idempotencyKey = request.IdempotencyKey ?? Guid.NewGuid().ToString("N");

        var jobId = BackgroundJob.Schedule<IExternalTaskJob>(
            job => job.ExecuteAsync(request.CallerCredentialId, request.TaskName, idempotencyKey, request.Payload, CancellationToken.None),
            TimeSpan.FromMinutes(request.DelayMinutes));

        return Task.FromResult(new ScheduleJobResult { JobId = jobId });
    }
}