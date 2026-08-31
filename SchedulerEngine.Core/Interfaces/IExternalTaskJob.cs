namespace SchedulerEngine.Core.Interfaces;

/// <summary>
/// Hangfire tarafından çalıştırılan, dış sisteme (FinYo, DocDes vb.) HTTP
/// isteği atan job'un soyutlaması. Somut implementasyon Infrastructure
/// katmanında (bkz. ExternalTaskJob.cs).
/// </summary>
public interface IExternalTaskJob
{
    /// <param name="callerCredentialId">
    /// Job'u kaydeden dış servisin (FinYo/DocDes) ApiKey credential'ının Id'si
    /// (ApiKeyAuthenticationHandler'ın enqueue anında set ettiği "credential_id"
    /// claim'inden geliyor). Job ÇALIŞIRKEN bu Id üzerinden hangi Organization'a
    /// ait olduğu, callback URL'i (ContactMedium.Url) ve giden istekte kullanılacak
    /// OutboundApiKey TAZE olarak veritabanından çözülür - appsettings'te sabit
    /// bir BaseUrl/ApiKey YOK, çünkü her çağıran servisin kendi adresi/anahtarı var.
    /// </param>
    /// <param name="idempotencyKey">
    /// Null geçilirse implementasyon her çalıştırmada YENİ bir anahtar üretir.
    /// Bunu ÖZELLİKLE recurring job'larda null bırak.
    /// </param>
    Task ExecuteAsync(
        Guid callerCredentialId,
        string taskName,
        string? idempotencyKey,
        Dictionary<string, object?> payload,
        CancellationToken ct);
}