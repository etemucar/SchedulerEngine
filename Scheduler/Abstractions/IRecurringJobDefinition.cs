namespace Scheduler.Abstractions;

/// <summary>
/// Sabit, kullanıcının açıp/kapatamadığı, sıklığını değiştiremediği sistem
/// job'ları için (örn. MarketInfo'daki QuoteIngestionJob — bkz.
/// MarketInfo.txt → "Job akışı"). Startup'ta DI'dan toplanıp
/// RecurringJobRegistrar tarafından Hangfire'a kaydedilir.
///
/// Kullanıcının kendi zamanlamasını yönetebildiği senaryolar (host'taki
/// Scheduler domain'i, ScheduledJob entity'si) bu arayüzü KULLANMAZ —
/// onun için doğrudan IRecurringJobManager (Hangfire'ın kendi arayüzü)
/// runtime'da inject edilip AddOrUpdate/RemoveIfExists/Trigger çağrılır.
/// Bkz. BackendContext.md → "Scheduler (Hangfire) Mimarisi" → Katman 2.
///
/// Host tarafında bir implementasyon örneği:
///
///   public sealed class QuoteIngestionJobDefinition : IRecurringJobDefinition
///   {
///       public string JobId => "quote-ingestion-bist";
///       public string CronExpression => "*/15 * * * *"; // 15 dakikada bir
///       public IScheduledCommand CreateCommand() => new RunQuoteIngestionCommand();
///   }
///
/// ve Program.cs'te (veya bir DI extension'ında) DI'ya eklenir:
///
///   services.AddSingleton&lt;IRecurringJobDefinition, QuoteIngestionJobDefinition&gt;();
/// </summary>
public interface IRecurringJobDefinition
{
    /// <summary>
    /// Hangfire'ın recurring job id'si olarak kullanılır. Deploy sonrası
    /// DEĞİŞTİRİLMEMELİ — değiştirilirse eski id ile duran job silinmez,
    /// yeni id ile ikinci bir kayıt oluşur (RemoveIfExists elle çağrılmadıkça).
    /// </summary>
    string JobId { get; }

    /// <summary>Standart 5 alanlı cron ifadesi (örn. "*/15 * * * *").</summary>
    string CronExpression { get; }

    /// <summary>
    /// Cron ifadesinin hangi saat diliminde değerlendirileceği. Belirtilmezse
    /// UTC varsayılır — BIST gibi yerel seans saatine bağlı job'lar için
    /// açıkça Europe/Istanbul verilmeli, aksi halde DST geçişlerinde kayma olur.
    /// </summary>
    TimeZoneInfo TimeZone => TimeZoneInfo.Utc;

    /// <summary>Tetiklenince gönderilecek MediatR command'ını üretir.</summary>
    IScheduledCommand CreateCommand();
}