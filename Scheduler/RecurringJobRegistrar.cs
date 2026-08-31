using Hangfire;
using Scheduler.Abstractions;
using Scheduler.Dispatch;

namespace Scheduler;

/// <summary>
/// Startup'ta DI'dan toplanan tüm IRecurringJobDefinition
/// implementasyonlarını Hangfire'ın recurring job store'una
/// AddOrUpdate ile kaydeder (idempotent — aynı JobId ile tekrar
/// çağrılırsa üzerine yazar, çift kayıt oluşmaz).
///
/// Program.cs'te, app build edildikten sonra bir kez çağrılır:
///
///   using (var scope = app.Services.CreateScope())
///   {
///       scope.ServiceProvider
///           .GetRequiredService&lt;RecurringJobRegistrar&gt;()
///           .RegisterAll();
///   }
///
/// Not: Bu sınıf sadece Katman 1'deki SABİT job'ları kaydeder. Kullanıcının
/// runtime'da kendi zamanlamasını ekleyip/kaldırdığı dinamik job'lar
/// (host'taki Scheduler domain'i, CreateScheduledJobCommandHandler) bu
/// sınıfı kullanmaz — doğrudan IRecurringJobManager'ı inject eder.
/// </summary>
public sealed class RecurringJobRegistrar
{
    private readonly IEnumerable<IRecurringJobDefinition> _definitions;
    private readonly IRecurringJobManager _recurringJobManager;

    public RecurringJobRegistrar(
        IEnumerable<IRecurringJobDefinition> definitions,
        IRecurringJobManager recurringJobManager)
    {
        _definitions = definitions;
        _recurringJobManager = recurringJobManager;
    }

    public void RegisterAll()
    {
        var seenIds = new HashSet<string>();

        foreach (var definition in _definitions)
        {
            // Aynı JobId'yi iki farklı IRecurringJobDefinition kullanırsa
            // (kopyala-yapıştır hatası vb.) sessizce üzerine yazmak yerine
            // erken patlat — startup'ta fark edilmesi, prod'da bir job'un
            // sessizce kaybolmasından çok daha ucuz.
            if (!seenIds.Add(definition.JobId))
            {
                throw new InvalidOperationException(
                    $"Duplicate recurring JobId detected: '{definition.JobId}'. " +
                    "Her IRecurringJobDefinition benzersiz bir JobId taşımalı.");
            }

            var command = definition.CreateCommand();

            _recurringJobManager.AddOrUpdate<MediatrCommandJob>(
                definition.JobId,
                job => job.Execute(command, CancellationToken.None),
                definition.CronExpression,
                new RecurringJobOptions { TimeZone = definition.TimeZone });
        }
    }
}