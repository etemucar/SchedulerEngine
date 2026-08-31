using Microsoft.EntityFrameworkCore;
using MediatR;
using FluentValidation;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Claims;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using SchedulerEngine.Infrastructure;
using SchedulerEngine.Service.Behaviors;
using SchedulerEngine.Settings.Core;
using SchedulerEngine.Api.Extensions;
using SchedulerEngine.Api.Security;
using SchedulerEngine.Core.Seeding;

var builder = WebApplication.CreateBuilder(args);

// ---- 1. Veritabanı Yapılandırması (PostgreSQL) ----
// Hangfire de AYNI connection string'i kullanıyor (ayrı "hangfire" şeması,
// aşağıya bakın) - DocDes'teki gibi ayrı bir HangfireConnection YOK.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection appsettings.json içinde tanımlı değil.");

builder.Services.AddDbContext<SchedulerEngineDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
        npgsqlOptions
            .MigrationsAssembly("SchedulerEngine.Infrastructure")
            .MigrationsHistoryTable("__EFMigrationsHistory"))
           .UseSnakeCaseNamingConvention());

// ---- 2. MediatR & Pipeline Behaviors ----
// NOT: typeof(TransactionalBehavior<,>).Assembly - bu class'ın gerçek
// namespace'i "SchedulerEngine.Service.Behaviors" olmayabilir, DocDes'teki
// "DocDes.Service.Behaviors"a bakarak tahmin ettim. Farklıysa using satırını
// düzeltin.
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(TransactionalBehavior<,>).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionalBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(TransactionalBehavior<,>).Assembly);

// ---- 3. AutoMapper ----
// NOT: SchedulerEngine.Service.Mapper.MappingProfile'ın var olduğunu
// varsayıyorum (DocDes'teki DocDes.Service.Mapper.MappingProfile'a bakarak).
// Service projenizde böyle bir sınıf yoksa bu satırı kaldırın - sadece
// ApiMappingProfile (typeof(Program).Assembly ile zaten taranıyor) yeterli olur.
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(new[]
    {
        typeof(SchedulerEngine.Service.Mapper.MappingProfile).Assembly,
        typeof(Program).Assembly
    });
});

// ---- 4. Ayarlar ve Uygulama Servisleri ----
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddApplicationServices(); // IRepository<,> açık generic kaydı BURADA - eksik olan buydu

// ---- 5. OpenAPI ----
builder.Services.AddOpenApi();

// ---- 6. CORS ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

// ---- 7. Controller ve JSON Optimizasyonları ----
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ---- 8. Altyapı Servisleri ----
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// JWT (cookie + Bearer header ikisini de destekliyor - bkz. ServiceCollectionExtensions.AddJwtAuthentication)
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// IExternalTaskJob, IEncryptionService kaydı ve HttpClient burada
// (SchedulerEngine.Infrastructure/DependencyInjection.cs).
builder.Services.AddInfrastructure(builder.Configuration);

// "SiteAdmin" policy - AdminController, AuthController.RegisterOrganization
// ve AdminOnlyDashboardAuthFilter (Hangfire dashboard) bunu kullanıyor.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SiteAdmin", policy =>
        policy.RequireClaim(ClaimTypes.Role, ReferenceDataIds.PartyRoleType.SiteAdminCd));
});

// ---- 9. Hangfire ----
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    // "SchedulerEngine" şeması SchedulerEngineDbContext'e ait (bkz.
    // EntityConfigurationBase.Schema); Hangfire kendi "hangfire" şemasını
    // kullanıyor - aynı connectionString'i paylaşsak bile çakışma olmaz.
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString),
        new PostgreSqlStorageOptions
        {
            SchemaName = "hangfire"
        }));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2;
    options.Queues = new[] { "default", "critical" };
});

// ---- 10. FinYo, DocDes gibi dış servislerin Job API'sine erişimi için ----
// API Key authentication (ayrı bir scheme, JWT'den bağımsız).
// AddAuthentication() burada parametresiz çağrılıyor - AddJwtAuthentication'ın
// (yukarıda, madde 8) kurduğu default scheme'i ETKİLEMEZ, sadece yeni bir
// scheme (ApiKey) ekler.
builder.Services.AddAuthentication()
    .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthConstants.SchemeName, options => { });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// NOT: UseAuthentication() UseAuthorization()'dan ÖNCE olmalı.
app.UseAuthentication();
app.UseAuthorization();

// ---- Hangfire Dashboard ----
// AdminOnlyDashboardAuthFilter, mevcut JWT + "SiteAdmin" policy'yi reuse
// ediyor - ayrı bir cookie/login sistemi YOK. UseAuthentication()/
// UseAuthorization()'dan SONRA map edilmeli ki HttpContext.User dolu gelsin.
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new AdminOnlyDashboardAuthFilter() }
});

app.MapControllers();

app.Run();