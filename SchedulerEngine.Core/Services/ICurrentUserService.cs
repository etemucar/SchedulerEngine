namespace SchedulerEngine.Core.Services;

public interface ICurrentUserService
{
    int? UserId { get; }

    Task<int> GetPartyRoleIdAsync(CancellationToken ct);
}
