using Hangfire;
using MediatR;
using SchedulerEngine.Core.Interfaces;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Handlers;

public class AddOrUpdateRecurringJobCommandHandler : IRequestHandler<AddOrUpdateRecurringJobCommand, RecurringJobResult>
{
    private readonly IRepository<Credential, Guid> _credentialRepository;
    private readonly IRepository<ServiceRecurringJob, int> _serviceRecurringJobRepository;

    public AddOrUpdateRecurringJobCommandHandler(
        IRepository<Credential, Guid> credentialRepository,
        IRepository<ServiceRecurringJob, int> serviceRecurringJobRepository)
    {
        _credentialRepository = credentialRepository;
        _serviceRecurringJobRepository = serviceRecurringJobRepository;
    }

    public async Task<RecurringJobResult> Handle(AddOrUpdateRecurringJobCommand request, CancellationToken ct)
    {
        var serviceName = await ResolveServiceNameAsync(request.CallerCredentialId, ct);
        var hangfireJobId = $"{serviceName.Replace(':', '-')}:{request.RecurringJobId}";

        var timeZone = string.IsNullOrWhiteSpace(request.TimeZoneId)
            ? TimeZoneInfo.Utc
            : TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId);

        // DÜZELTME (2026-09): IExternalTaskJob.ExecuteAsync'e PerformContext
        // parametresi eklendi (bkz. IExternalTaskJob.cs, ExternalTaskJob.cs) —
        // idempotencyKey'in retry'lar arasında sabit kalabilmesi için gerekli.
        // Buraya geçilen `null`, Hangfire'ın PerformContext tipi için
        // kendi konvansiyonu: registration anında ne yazarsanız yazın,
        // Hangfire bu parametreyi TİP eşleşmesine göre tanıyıp her execution'da
        // (retry dahil) gerçek PerformContext ile OTOMATİK değiştirir.
        RecurringJob.AddOrUpdate<IExternalTaskJob>(
            hangfireJobId,
            job => job.ExecuteAsync(request.CallerCredentialId, request.TaskName, null, request.Payload, null, CancellationToken.None),
            request.CronExpression,
            new RecurringJobOptions { TimeZone = timeZone });

        await UpsertAuditRecordAsync(request, serviceName, hangfireJobId, ct);

        return new RecurringJobResult
        {
            RecurringJobId = request.RecurringJobId,
            CronExpression = request.CronExpression
        };
    }

    private async Task UpsertAuditRecordAsync(
        AddOrUpdateRecurringJobCommand request,
        string serviceName,
        string hangfireJobId,
        CancellationToken ct)
    {
        var existing = await _serviceRecurringJobRepository.FindOneAsync(
            x => x.CallerCredentialId == request.CallerCredentialId && x.RecurringJobId == request.RecurringJobId,
            asNoTracking: false,
            ct: ct);

        if (existing is null)
        {
            await _serviceRecurringJobRepository.AddAsync(new ServiceRecurringJob
            {
                CallerCredentialId = request.CallerCredentialId,
                ServiceName = serviceName,
                RecurringJobId = request.RecurringJobId,
                HangfireJobId = hangfireJobId,
                CronExpression = request.CronExpression,
                TimeZoneId = request.TimeZoneId,
                TaskName = request.TaskName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }, ct);
        }
        else
        {
            existing.CronExpression = request.CronExpression;
            existing.TimeZoneId = request.TimeZoneId;
            existing.TaskName = request.TaskName;
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.RemovedAt = null;

            await _serviceRecurringJobRepository.UpdateAsync(existing, ct);
        }
    }

    private async Task<string> ResolveServiceNameAsync(Guid callerCredentialId, CancellationToken ct)
    {
        var serviceName = await _credentialRepository.FindOneSelectAsync(
            c => c.Id == callerCredentialId,
            c => c.DigitalIdentity.PartyRole.Party.Organization != null
                ? c.DigitalIdentity.PartyRole.Party.Organization!.Name
                : null,
            ct: ct);

        if (string.IsNullOrEmpty(serviceName))
        {
            throw new InvalidOperationException(
                $"Çağıran credential için servis adı çözülemedi. CredentialId: {callerCredentialId}");
        }

        return serviceName;
    }
}