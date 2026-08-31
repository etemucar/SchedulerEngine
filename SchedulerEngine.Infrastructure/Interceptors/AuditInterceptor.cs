using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SchedulerEngine.Core.Services;
using SchedulerEngine.Core.Base;

public sealed class AuditInterceptor : ISaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditInterceptor(ICurrentUserService currentUserService)
        => _currentUserService = currentUserService;

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAudit(eventData.Context);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    private void UpdateAudit(DbContext? context)
    {
        if (context == null) return;

        var currentUserId = _currentUserService.UserId ?? 0;
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<ModelBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreateDate = now;
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.UpdateDate = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdateDate = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    break;
            }
        }
    }

    // Diğer SaveChangesAsync overload’ları da aynı şekilde
}
