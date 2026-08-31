// Infrastructure/DependencyInjection.cs

using SchedulerEngine.Core.Interfaces;
using SchedulerEngine.Core.Security;
using SchedulerEngine.Infrastructure.Security;
using SchedulerEngine.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SchedulerEngine.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();

        services.AddScoped<IExternalTaskJob, ExternalTaskJob>();

        services.AddSingleton<IEncryptionService, EncryptionService>();

        services.AddMemoryCache();

        return services;
    }
}