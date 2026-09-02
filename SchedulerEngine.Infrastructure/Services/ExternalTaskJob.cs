using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Enums;
using SchedulerEngine.Core.Interfaces;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Security;

namespace SchedulerEngine.Infrastructure.Services;

/// <summary>
/// Hangfire tarafından çağrılan somut job. Dış sisteme (FinYo, DocDes vb.)
/// HTTP isteği atar - hedef URL ve gönderilecek anahtar appsettings'te SABİT
/// DEĞİL, callerCredentialId üzerinden her çalıştırmada veritabanından TAZE
/// çözülür (bkz. IExternalTaskJob.ExecuteAsync dokümantasyonu).
/// [AutomaticRetry]: geçici hatalarda otomatik tekrar dener.
/// </summary>
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
        CancellationToken ct)
    {
        // idempotencyKey null geldiyse (recurring job'lar için) burada, her
        // gerçek çalıştırmada taze bir tane üretiyoruz.
        idempotencyKey ??= Guid.NewGuid().ToString("N");

        // Çağıranın (FinYo/DocDes'in bize kimlik doğrularken kullandığı ApiKey
        // credential'ının) ContactMedium'undan (Url) ve aynı DigitalIdentity'nin
        // kardeş bir OutboundApiKey credential'ından çağrı bilgilerini çözüyoruz.
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

        var client = _httpClientFactory.CreateClient(); // isimli/BaseAddress'li değil - URL her seferinde tam olarak veriliyor

        if (!string.IsNullOrEmpty(outboundEncrypted))
        {
            var rawOutboundKey = _encryptionService.Decrypt(outboundEncrypted);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawOutboundKey);
        }
        else
        {
            _logger.LogWarning(
                "Bu servis için OutboundApiKey tanımlı değil, Authorization header'sız gönderiliyor. CredentialId: {CredentialId}",
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

        response.EnsureSuccessStatusCode(); // hata -> exception -> Hangfire retry tetiklenir

        _logger.LogInformation("Görev başarıyla dış sisteme iletildi. TaskName={TaskName}", taskName);
    }
}