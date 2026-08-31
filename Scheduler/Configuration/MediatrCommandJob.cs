using MediatR;
using Scheduler.Abstractions;

namespace Scheduler.Dispatch;

/// <summary>
/// Hangfire'ın çağırdığı TEK generic job sınıfı — her yeni job türü için
/// ayrı bir Hangfire job class'ı yazılmaz, hepsi IScheduledCommand
/// implementasyonu üzerinden bu tek metoda akar.
///
/// Serileştirme notu: Hangfire, Execute(command, ct) çağrısını job storage'a
/// yazarken `command` parametresini JSON'a serialize eder; tetiklendiğinde
/// deserialize edip tekrar bu metoda geçirir. Parametre tipi arayüz
/// (IScheduledCommand) olduğu için Hangfire'ın recommended serializer
/// ayarları (UseRecommendedSerializerSettings, bkz.
/// HangfireConfigurationExtensions) polimorfik tip bilgisini ($type)
/// otomatik gömer — aksi halde deserialize aşamasında arayüzü somutlaştıramaz.
/// </summary>
public sealed class MediatrCommandJob
{
    private readonly IMediator _mediator;

    public MediatrCommandJob(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task Execute(IScheduledCommand command, CancellationToken cancellationToken)
    {
        return command.ExecuteAsync(_mediator, cancellationToken);
    }
}