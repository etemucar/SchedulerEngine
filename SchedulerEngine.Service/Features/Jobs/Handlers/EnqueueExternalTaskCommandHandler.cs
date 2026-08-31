using Hangfire;
using MediatR;
using SchedulerEngine.Core.Interfaces;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Handlers;

public class EnqueueExternalTaskCommandHandler : IRequestHandler<EnqueueExternalTaskCommand, EnqueueJobResult>
{
    public Task<EnqueueJobResult> Handle(EnqueueExternalTaskCommand request, CancellationToken ct)
    {
        var idempotencyKey = request.IdempotencyKey ?? Guid.NewGuid().ToString("N");

        var jobId = BackgroundJob.Enqueue<IExternalTaskJob>(
            job => job.ExecuteAsync(request.CallerCredentialId, request.TaskName, idempotencyKey, request.Payload, CancellationToken.None));

        return Task.FromResult(new EnqueueJobResult { JobId = jobId });
    }
}