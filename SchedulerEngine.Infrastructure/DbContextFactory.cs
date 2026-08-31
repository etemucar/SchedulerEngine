using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SchedulerEngine.Infrastructure;
public class SchedulerEngineDbContextFactory : IDesignTimeDbContextFactory<SchedulerEngineDbContext>
{
    public SchedulerEngineDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string not found.");

        var optionsBuilder = new DbContextOptionsBuilder<SchedulerEngineDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new SchedulerEngineDbContext(optionsBuilder.Options);
    }
}
