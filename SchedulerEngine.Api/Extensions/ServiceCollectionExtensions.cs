using System.Text;
using System.Text.Json;
using SchedulerEngine.Core.Data;
using SchedulerEngine.Core.Responses;
using SchedulerEngine.Infrastructure;
using SchedulerEngine.Api.Services;
using SchedulerEngine.Core.Services;
using SchedulerEngine.Core.Security;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Infrastructure.Repositories;
using SchedulerEngine.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SchedulerEngine.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var tokenOptions = configuration
            .GetSection("AppSettings:TokenOptions")
            .Get<TokenOptions>()
            ?? throw new InvalidOperationException("TokenOptions yapılandırması bulunamadı.");

        var key = Encoding.ASCII.GetBytes(tokenOptions.SecurityKey);

        // ForbiddenException/ErrorResponse ile aynı JSON şeklini üretmek için —
        // ExceptionMiddleware'deki JsonSerializerOptions ile tutarlı.
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Development. Production'da true yapın.
                options.SaveToken            = false; // Token HttpContext.User'da tutulmasın — cookie'den okunuyor.

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = tokenOptions.Issuer,
                    ValidAudience            = tokenOptions.Audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(key),
                    ClockSkew                = TimeSpan.Zero // Token süresi tam olarak kontrol edilsin.
                };

                // Token önce Authorization header'dan, yoksa HttpOnly cookie'den okunur.
                // Bu sayede hem web (cookie) hem de API/mobil (Bearer header) desteklenir.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        // 1. Önce Authorization: Bearer <token> header'ına bak.
                        var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                            return Task.CompletedTask; // Header varsa cookie'ye gerek yok.

                        // 2. Header yoksa HttpOnly cookie'den oku.
                        var cookieToken = ctx.Request.Cookies["auth"];
                        if (!string.IsNullOrEmpty(cookieToken))
                            ctx.Token = cookieToken;

                        return Task.CompletedTask;
                    },

                    // 401 dönerken WWW-Authenticate header'ını temizle —
                    // tarayıcı gereksiz Basic Auth popup açmasın.
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        ctx.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                        ctx.Response.ContentType = "application/json";
                        return ctx.Response.WriteAsync(
                            JsonSerializer.Serialize(
                                new ErrorResponse("Yetkisiz erişim. Lütfen giriş yapın."),
                                jsonOptions));
                    },

                    // 403 için ExceptionMiddleware'deki ErrorResponse ile aynı format.
                    OnForbidden = ctx =>
                    {
                        ctx.Response.StatusCode  = StatusCodes.Status403Forbidden;
                        ctx.Response.ContentType = "application/json";
                        return ctx.Response.WriteAsync(
                            JsonSerializer.Serialize(
                                new ErrorResponse("Bu işlem için yetkiniz yok."),
                                jsonOptions));
                    }
                };
            });

        return services;
    }
}
