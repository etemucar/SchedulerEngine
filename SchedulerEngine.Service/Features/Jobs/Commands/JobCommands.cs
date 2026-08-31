using MediatR;
using SchedulerEngine.Service.Dtos.Responses;

namespace SchedulerEngine.Service.Features.Commands;

/// <summary>Job'u hemen kuyruğa alır.</summary>
public record EnqueueExternalTaskCommand : IRequest<EnqueueJobResult>
{
    /// <summary>ApiKeyAuthenticationHandler'ın set ettiği "credential_id" claim'inden - job çalışırken hangi Organization'a ait olduğunu çözmek için.</summary>
    public Guid CallerCredentialId { get; init; }
    public string TaskName { get; init; } = null!;
    public Dictionary<string, object?> Payload { get; init; } = new();
    public string? IdempotencyKey { get; init; }
}

/// <summary>Job'u belirli bir gecikmeyle çalıştırır.</summary>
public record ScheduleExternalTaskCommand : IRequest<ScheduleJobResult>
{
    public Guid CallerCredentialId { get; init; }
    public string TaskName { get; init; } = null!;
    public Dictionary<string, object?> Payload { get; init; } = new();
    public string? IdempotencyKey { get; init; }
    public int DelayMinutes { get; init; }
}

/// <summary>Cron ifadesiyle tekrarlayan bir job tanımlar/günceller.</summary>
public record AddOrUpdateRecurringJobCommand : IRequest<RecurringJobResult>
{
    public Guid CallerCredentialId { get; init; }
    public string RecurringJobId { get; init; } = null!;
    public string CronExpression { get; init; } = null!;
    public string TaskName { get; init; } = null!;
    public Dictionary<string, object?> Payload { get; init; } = new();

    /// <summary>
    /// IANA saat dilimi id'si (örn. "Europe/Istanbul"). Boş bırakılırsa UTC
    /// kullanılır - cron ifadesindeki saatler her zaman BU saat dilimine
    /// göre yorumlanır, caller kendi yerel saatini UTC'ye çevirmek zorunda
    /// kalmaz.
    /// </summary>
    public string? TimeZoneId { get; init; }
}

/// <summary>Mevcut bir recurring job'ı kaldırır.</summary>
public record RemoveRecurringJobCommand : IRequest<bool>
{
    public Guid CallerCredentialId { get; init; }
    public string RecurringJobId { get; init; } = null!;
}