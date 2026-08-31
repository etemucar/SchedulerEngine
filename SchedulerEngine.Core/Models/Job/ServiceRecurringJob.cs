using SchedulerEngine.Core.Base;

namespace SchedulerEngine.Core.Model;

/// <summary>
/// Hangi servisin (Credential üzerinden) hangi recurring job'u Hangfire'a
/// kaydettiğinin audit/listeleme kaydı. Gerçek zamanlama bilgisi (cron,
/// timezone) Hangfire'ın KENDİ "hangfire" şemasında saklanıyor - bu tablo
/// SADECE "kim, ne zaman, ne kaydetti" sorgusunu cevaplamak için, Hangfire'ın
/// yerini almıyor.
/// </summary>
public class ServiceRecurringJob : ModelBase<int>
{
    public Guid CallerCredentialId { get; set; }

    /// <summary>Denormalize edilmiş servis adı - her sorguda join'e gerek kalmasın diye.</summary>
    public string ServiceName { get; set; } = null!;

    /// <summary>Caller'ın kendi verdiği, PREFIX'SİZ id (Hangfire'daki gerçek id değil).</summary>
    public string RecurringJobId { get; set; } = null!;

    /// <summary>Hangfire'a kaydedilen gerçek, "{ServiceName}:{RecurringJobId}" formatındaki id.</summary>
    public string HangfireJobId { get; set; } = null!;

    public string CronExpression { get; set; } = null!;
    public string? TimeZoneId { get; set; }
    public string TaskName { get; set; } = null!;

    /// <summary>Hâlâ Hangfire'da kayıtlı mı - false ise RemoveRecurringJobCommand ile kaldırılmış.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
}