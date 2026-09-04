using System.Net.Http.Json;
using Hangfire;
using Hangfire.Server; // PerformContext için
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Enums;
using SchedulerEngine.Core.Interfaces;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Security;

namespace SchedulerEngine.Infrastructure.Services;

public class ExternalTaskJob : IExternalTaskJob
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRepository<Credential, Guid> _credentialRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<ExternalTaskJob> _logger;

    public ExternalTaskJob(
        IHttpClientFactory httpClientFactory,
        IRepository<Credential, Guid> credentialRepository,
        IEncryptionService encryptionService,
        ILogger<ExternalTaskJob> logger)
    {
        _httpClientFactory = httpClientFactory;
        _credentialRepository = credentialRepository;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 15, 60, 300 })]
    public async Task ExecuteAsync(
        Guid callerCredentialId,
        string taskName,
        string? idempotencyKey,
        Dictionary<string, object?> payload,
        PerformContext? context, // Hangfire runtime'da otomatik dolduruyor, job parametresi olarak SAKLANMIYOR
        CancellationToken ct)
    {
        // DÜZELTME (2026-09): Eskiden burada `idempotencyKey ??= Guid.NewGuid()...`
        // vardı — bu, idempotencyKey null geldiğinde (tipik recurring job
        // senaryosu) HER retry'da YENİ bir rastgele değer üretiyordu, çünkü
        // Guid.NewGuid() her çağrıda farklı sonuç verir. Sonuç: FinYo/DocDes
        // tarafındaki duplicate-kontrolü hiçbir zaman eşleşme bulamıyordu,
        // idempotency FİİLEN çalışmıyordu.
        //
        // Artık: idempotencyKey null ise, Hangfire'ın bu job ÇALIŞTIRMASINA
        // (occurrence'ına) verdiği JobId kullanılıyor. JobId, Hangfire
        // tarafından retry'lar arasında SABİT kalır (aynı job'ın retry'ı,
        // yeni bir job değildir) — bu yüzden 1. denemede de, ağ hatası
        // sonucu gerçekleşen 2./3. retry'de de aynı anahtar FinYo'ya gider.
        //
        // NOT: context?.BackgroundJob?.Id, job parametresi DEĞİL — Hangfire
        // tarafından metot her çağrıldığında runtime'da enjekte edilir, bu
        // yüzden DB'de saklanan serileştirilmiş argümanların bir parçası
        // değildir ve her retry'da "taze" ama JobId sabit olduğu için AYNI
        // değeri üretir.
        idempotencyKey ??= $"hangfire-job-{context?.BackgroundJob?.Id ?? Guid.NewGuid().ToString("N")}";

        var callerCredential = await _credentialRepository.FindOneAsync(
            c => c.Id == callerCredentialId,
            include: q => q
                .Include(c => c.ContactMedia)
                .Include(c => c.DigitalIdentity),
            asNoTracking: true,
            ct: ct);

        if (callerCredential is null)
        {
            throw new InvalidOperationException(
                $"Çağıran credential bulunamadı. CredentialId: {callerCredentialId}. " +
                "Bu servis silinmiş veya credential'ı kaldırılmış olabilir.");
        }

        var callbackUrl = callerCredential.ContactMedia
            .FirstOrDefault(cm => cm.MediumType == ContactMediumType.Url)?.Url;

        if (string.IsNullOrEmpty(callbackUrl))
        {
            throw new InvalidOperationException(
                $"Bu servis için callback URL (ContactMedium.Url) tanımlı değil. CredentialId: {callerCredentialId}");
        }

        var outboundCredential = await _credentialRepository.FindOneAsync(
            c => c.DigitalIdentityId == callerCredential.DigitalIdentityId
                 && c.CredentialType == CredentialType.OutboundApiKey,
            include: q => q.Include(c => c.Characteristics),
            asNoTracking: true,
            ct: ct);

        var outboundEncrypted = outboundCredential?.Characteristics
            .FirstOrDefault(ch => ch.Name == "outboundApiKeyEncrypted")?.Value;

        var client = _httpClientFactory.CreateClient();

        if (!string.IsNullOrEmpty(outboundEncrypted))
        {
            // DÜZELTME (2026-09): Eskiden burada `Authorization: Bearer <key>`
            // gönderiliyordu. Ama FinYo/DocDes tarafındaki SchedulerWebhookController
            // kimlik doğrulamasını "X-Outbound-Api-Key" header'ından okuyor — iki
            // taraf hiç eşleşmiyordu, her çağrı 401 ile dönüyordu (host tarafında
            // ilk başta bu, eksik/boş environment variable sanılmıştı; asıl neden
            // header adı uyuşmazlığıydı). Ayrıca FinYo tarafında kullanıcı JWT auth'u
            // da "Authorization: Bearer" kullandığı için, aynı header'ı servisler
            // arası API key için de kullanmak iki farklı auth şemasını çakıştırıp
            // JwtBearer middleware'inin bu değeri JWT olarak parse etmeye çalışmasına
            // yol açabilirdi. Bu yüzden ayrı, özel bir header'a geçildi.
            var rawOutboundKey = _encryptionService.Decrypt(outboundEncrypted);
            client.DefaultRequestHeaders.Add("X-Outbound-Api-Key", rawOutboundKey);
        }
        else
        {
            _logger.LogWarning(
                "Bu servis için OutboundApiKey tanımlı değil, X-Outbound-Api-Key header'sız gönderiliyor. CredentialId: {CredentialId}",
                callerCredentialId);
        }

        client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);

        _logger.LogInformation(
            "Dış sisteme görev gönderiliyor. Url={Url} TaskName={TaskName} IdempotencyKey={IdempotencyKey}",
            callbackUrl, taskName, idempotencyKey);

        var response = await client.PostAsJsonAsync(callbackUrl, new
        {
            taskName,
            payload,
            idempotencyKey
        }, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning(
                "Dış sistem hata döndü. Status={StatusCode} Body={Body}",
                response.StatusCode, body);
        }

        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Görev başarıyla dış sisteme iletildi. TaskName={TaskName}", taskName);
    }
}