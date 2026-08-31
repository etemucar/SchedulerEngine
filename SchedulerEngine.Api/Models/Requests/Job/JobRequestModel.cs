using System.ComponentModel.DataAnnotations;

namespace SchedulerEngine.Api.Models;

/// <summary>Job kaydı için istek modeli (TMF dışı - Auth/Admin ile aynı konvansiyon).</summary>
public class ExternalTaskRequestModel
{
    [Required(ErrorMessage = "TaskName zorunludur.")]
    [MaxLength(200)]
    public string TaskName { get; set; } = null!;

    public Dictionary<string, object?> Payload { get; set; } = new();

    /// <summary>Boş bırakılırsa Command katmanında otomatik üretilir.</summary>
    public string? IdempotencyKey { get; set; }
}

public class ScheduleExternalTaskRequestModel : ExternalTaskRequestModel
{
    [Range(1, int.MaxValue, ErrorMessage = "DelayMinutes 0'dan büyük olmalı.")]
    public int DelayMinutes { get; set; }
}

public class RecurringJobRequestModel
{
    [Required]
    [MaxLength(200)]
    public string RecurringJobId { get; set; } = null!;

    [Required]
    public string CronExpression { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string TaskName { get; set; } = null!;

    public Dictionary<string, object?> Payload { get; set; } = new();

    /// <summary>
    /// IANA saat dilimi id'si (örn. "Europe/Istanbul"). Boş bırakılırsa UTC
    /// kullanılır. CronExpression'daki saatler bu saat dilimine göre
    /// yorumlanır - kendi yerel saatinizi UTC'ye çevirmenize gerek yok.
    /// </summary>
    public string? TimeZoneId { get; set; }
}