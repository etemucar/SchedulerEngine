using Hangfire.Server;

namespace SchedulerEngine.Core.Interfaces;

/// <summary>
/// Hangfire tarafından çalıştırılan, dış sisteme (FinYo, DocDes vb.) HTTP
/// isteği atan job'un soyutlaması. Somut implementasyon Infrastructure
/// katmanında (bkz. ExternalTaskJob.cs).
/// </summary>
public interface IExternalTaskJob
{
    /// <param name="callerCredentialId">
    /// Job'u kaydeden dış servisin (FinYo/DocDes) ApiKey credential'ının Id'si.
    /// </param>
    /// <param name="idempotencyKey">
    /// 2026-09 GÜNCELLEME — ESKİ DAVRANIŞ YANLIŞTI: "Null geçilirse her
    /// çalıştırmada YENİ bir anahtar üretir" ifadesi, retry'larda da yeni
    /// (rastgele) bir değer üretildiği için idempotency'yi fiilen işlevsiz
    /// bırakıyordu.
    ///
    /// YENİ DAVRANIŞ: Null geçilirse implementasyon, Hangfire'ın bu job
    /// ÇALIŞTIRMASINA (occurrence) verdiği JobId'yi kullanır
    /// (context.BackgroundJob.Id). JobId, aynı occurrence'ın retry'ları
    /// arasında SABİT kalır (Hangfire yeni bir job açmaz, aynı JobId ile
    /// tekrar dener) — bu yüzden null bırakmak recurring job'lar için HÂLÂ
    /// doğru ve önerilen kullanım: her yeni tetikleniş (yeni JobId) yeni bir
    /// idempotency key üretir, ama o tetiklenişin retry'ları aynı key'i
    /// paylaşır.
    /// </param>
    /// <param name="context">
    /// Hangfire tarafından runtime'da otomatik enjekte edilir — job'u
    /// kaydederken (Enqueue/RecurringJob.AddOrUpdate) bu parametre için
    /// DEĞER VERİLMEMELİDİR, Hangfire'ın kendisi doldurur.
    /// </param>
    Task ExecuteAsync(
        Guid callerCredentialId,
        string taskName,
        string? idempotencyKey,
        Dictionary<string, object?> payload,
        PerformContext? context,
        CancellationToken ct);
}