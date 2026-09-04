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
        // idempotencyKey burada, job oluşturulmadan ÖNCE sabitleniyor —
        // Hangfire bu değeri job argümanı olarak saklıyor, bu yüzden bu
        // job'un TÜM retry'larında aynı kalıyor (recurring job'lardaki gibi
        // null bırakıp ExecuteAsync içindeki PerformContext fallback'ine
        // güvenmeye gerek yok, çünkü burada zaten tek seferlik/gecikmeli
        // bir job var, "yeni occurrence" kavramı yok).
        var idempotencyKey = request.IdempotencyKey ?? Guid.NewGuid().ToString("N");

        var jobId = BackgroundJob.Schedule<IExternalTaskJob>(
            job => job.ExecuteAsync(request.CallerCredentialId, request.TaskName, idempotencyKey, request.Payload, null, CancellationToken.None),
            TimeSpan.FromMinutes(request.DelayMinutes));

        return Task.FromResult(new ScheduleJobResult { JobId = jobId });
    }
}