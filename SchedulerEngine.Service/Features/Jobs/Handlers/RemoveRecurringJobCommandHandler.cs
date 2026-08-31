using Hangfire;
using MediatR;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Handlers;

public class RemoveRecurringJobCommandHandler : IRequestHandler<RemoveRecurringJobCommand, bool>
{
    private readonly IRepository<Credential, Guid> _credentialRepository;
    private readonly IRepository<ServiceRecurringJob, int> _serviceRecurringJobRepository;

    public RemoveRecurringJobCommandHandler(
        IRepository<Credential, Guid> credentialRepository,
        IRepository<ServiceRecurringJob, int> serviceRecurringJobRepository)
    {
        _credentialRepository = credentialRepository;
        _serviceRecurringJobRepository = serviceRecurringJobRepository;
    }

    public async Task<bool> Handle(RemoveRecurringJobCommand request, CancellationToken ct)
    {
        // AddOrUpdateRecurringJobCommandHandler'daki ile AYNI prefix mantığı -
        // gerçek Hangfire id'sini yeniden üretmemiz lazım, yoksa caller'ın
        // kendi (prefix'siz) id'siyle silme denemesi hiçbir şeyi bulamaz.
        // Bu aynı zamanda örtük bir sahiplik kontrolü: FinYo, DocDes'in
        // job'unu SADECE kendi ServiceName'iyle prefix'lenmiş id'yi tahmin
        // edip üretebilseydi silebilirdi - ki bunu yapamaz çünkü prefix
        // request.CallerCredentialId'den (FinYo'nun KENDİ credential'ı)
        // çözülüyor, request body'de gelen serbest bir alan değil.
        var serviceName = await ResolveServiceNameAsync(request.CallerCredentialId, ct);
        var hangfireJobId = $"{serviceName.Replace(':', '-')}:{request.RecurringJobId}";

        RecurringJob.RemoveIfExists(hangfireJobId);

        // Audit kaydı - soft delete (IsActive=false). Hard delete YOK,
        // "audit" isteğinin amacı geçmişi de görebilmek.
        await SoftDeleteAuditRecordAsync(request.CallerCredentialId, request.RecurringJobId, ct);

        // Not: Hangfire'ın RemoveIfExists metodu, kaldırma öncesinde job'un
        // var olup olmadığını dönmez (void). Bu yüzden burada her zaman
        // "true" dönüyoruz - "işlem denendi" anlamında, "kayıt zaten
        // vardı/silindi" garantisi değil.
        return true;
    }

    private async Task SoftDeleteAuditRecordAsync(Guid callerCredentialId, string recurringJobId, CancellationToken ct)
    {
        var existing = await _serviceRecurringJobRepository.FindOneAsync(
            x => x.CallerCredentialId == callerCredentialId && x.RecurringJobId == recurringJobId,
            asNoTracking: false,
            ct: ct);

        // Kayıt yoksa (bu audit özelliği eklenmeden önce oluşturulmuş bir job
        // olabilir) sessizce geç - Hangfire tarafında zaten RemoveIfExists denendi.
        if (existing is null)
            return;

        existing.IsActive = false;
        existing.RemovedAt = DateTime.UtcNow;

        await _serviceRecurringJobRepository.UpdateAsync(existing, ct);
    }

    /// <summary>AddOrUpdateRecurringJobCommandHandler'daki ile bilinçli tekrar.</summary>
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