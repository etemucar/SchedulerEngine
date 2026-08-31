using MediatR;

namespace Scheduler.Abstractions;

/// <summary>
/// Scheduler.csproj'un host'un (FinYo/DocDes) MediatR command'larını
/// tanımadan tetikleyebilmesini sağlayan sarmalayıcı arayüz.
///
/// Host tarafında (Core veya Service katmanında) küçük bir implementasyon
/// yazılır, örn.:
///
///   public sealed class RunQuoteIngestionCommand : IScheduledCommand
///   {
///       public Task ExecuteAsync(IMediator mediator, CancellationToken ct)
///           => mediator.Send(new QuoteIngestionCommand(), ct);
///   }
///
/// Bu tip, Hangfire tarafından JSON'a serialize edilip deserialize edilir
/// (bkz. MediatrCommandJob) — bu yüzden implementasyonların parametresiz
/// (ya da sadece basit, serialize edilebilir alanlar taşıyan) POCO'lar
/// olması gerekir; ctor injection YAPILAMAZ (Hangfire storage'dan
/// deserialize ederken DI konteynerini kullanmaz).
/// </summary>
public interface IScheduledCommand
{
    Task ExecuteAsync(IMediator mediator, CancellationToken cancellationToken);
}