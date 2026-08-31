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
        // FinYo/DocDes gibi iki farklı servis aynı recurringJobId'yi seçerse
        // (örn. "daily-sync"), Hangfire'ın DÜZ (tek) isim alanında biri
        // diğerini sessizce ezer. Bunu önlemek için gerçek Hangfire id'sini
        // "{ServiceName}:{recurringJobId}" olarak prefix'liyoruz - caller
        // buna dair hiçbir şey bilmiyor, kendi verdiği id ile çalışmaya
        // devam ediyor (response'ta da kendi id'sini görüyor).
        var serviceName = await ResolveServiceNameAsync(request.CallerCredentialId, ct);
        var hangfireJobId = $"{serviceName.Replace(':', '-')}:{request.RecurringJobId}";

        // TimeZoneId boşsa UTC - cron ifadesindeki saatler bu saat dilimine
        // göre yorumlanır (Validator, geçersiz id'leri daha önce eledi).
        var timeZone = string.IsNullOrWhiteSpace(request.TimeZoneId)
            ? TimeZoneInfo.Utc
            : TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId);

        RecurringJob.AddOrUpdate<IExternalTaskJob>(
            hangfireJobId,
            job => job.ExecuteAsync(request.CallerCredentialId, request.TaskName, null, request.Payload, CancellationToken.None),
            request.CronExpression,
            new RecurringJobOptions { TimeZone = timeZone });

        // Audit kaydı - Hangfire'ın kendi storage'ının YANI SIRA, kendi
        // tablomuzda da "kim, ne zaman, ne kaydetti" bilgisini tutuyoruz
        // (listeleme/audit endpoint'i için, bkz. GetMyRecurringJobsQuery).
        await UpsertAuditRecordAsync(request, serviceName, hangfireJobId, ct);

        return new RecurringJobResult
        {
            RecurringJobId = request.RecurringJobId, // caller'ın kendi verdiği, PREFIX'SİZ id
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
            existing.RemovedAt = null; // daha önce kaldırılmışsa (soft-delete), tekrar aktif hale geliyor

            await _serviceRecurringJobRepository.UpdateAsync(existing, ct);
        }
    }

    /// <summary>
    /// RemoveRecurringJobCommandHandler'da da aynısı var (bilinçli tekrar -
    /// bu katmandaki diğer Map* yardımcı metotları da aynı şekilde
    /// duplike ediliyor, bkz. MapContactMedium).
    /// </summary>
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