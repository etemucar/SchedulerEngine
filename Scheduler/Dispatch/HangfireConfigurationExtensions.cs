using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Dispatch;

namespace Scheduler.Configuration;

/// <summary>
/// Scheduler.csproj'un DI/pipeline kurulum noktası. FinYo.Api ve DocDes.Api
/// Program.cs'lerinde BİREBİR aynı şekilde çağrılır.
/// </summary>
public static class HangfireConfigurationExtensions
{
    /// <summary>
    /// Hangfire'ı PostgreSQL storage ile kurar (kendi hangfire.* şeması,
    /// EF Core migration'larından tamamen bağımsız — bkz. BackendContext.md
    /// → "Scheduler (Hangfire) Mimarisi" → Katman 1), MediatrCommandJob'u ve
    /// RecurringJobRegistrar'ı DI'ya ekler.
    ///
    /// connectionString: host'un zaten kullandığı Postgres connection string
    /// (aynı veritabanı, farklı şema — ayrı bir Hangfire veritabanına gerek yok).
    /// </summary>
    public static IServiceCollection AddScheduler(
        this IServiceCollection services,
        string connectionString,
        string schemaName = "hangfire")
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            // Recommended settings, IScheduledCommand gibi arayüz-tipli
            // job argümanlarının $type bilgisiyle serialize/deserialize
            // edilebilmesi için gerekli (bkz. MediatrCommandJob).
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                // NOT — versiyona duyarlı API: Hangfire.PostgreSql 1.20+
                // NpgsqlConnection factory-tabanlı bu imzayı kullanır.
                // Yüklenen paket sürümü farklıysa (örn. 1.9.x string-tabanlı
                // eski imza), bu satırı o sürümün API'sine göre güncelleyin.
                options => options.UseNpgsqlConnection(connectionString),
                new PostgreSqlStorageOptions
                {
                    SchemaName = schemaName,
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(15)
                }));

        services.AddHangfireServer();

        services.AddScoped<MediatrCommandJob>();
        services.AddSingleton<RecurringJobRegistrar>();

        return services;
    }

    /// <summary>
    /// Hangfire dashboard'unu bir yetkilendirme filtresinin arkasına koyar.
    /// Filtresiz (anonim erişime açık) dashboard PROD'da asla kullanılmamalı.
    ///
    /// Örnek kullanım (Program.cs, app build edildikten sonra):
    ///   app.UseSchedulerDashboard("/hangfire", new AdminOnlyDashboardAuthFilter());
    ///
    /// AdminOnlyDashboardAuthFilter, host tarafında yazılır (Api katmanında) —
    /// örn. mevcut ClaimTypes.Role tabanlı policy'yi HttpContext üzerinden
    /// kontrol eden basit bir IDashboardAuthorizationFilter implementasyonu.
    /// </summary>
    public static IApplicationBuilder UseSchedulerDashboard(
        this IApplicationBuilder app,
        string path,
        IDashboardAuthorizationFilter authFilter)
    {
        return app.UseHangfireDashboard(path, new DashboardOptions
        {
            Authorization = new[] { authFilter }
        });
    }
}