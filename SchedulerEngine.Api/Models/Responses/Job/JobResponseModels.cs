namespace SchedulerEngine.Api.Models;

public class EnqueueJobResponse
{
    public bool Success { get; set; } = true;
    public string JobId { get; set; } = null!;
}

public class ScheduleJobResponse
{
    public bool Success { get; set; } = true;
    public string JobId { get; set; } = null!;
}

public class RecurringJobResponse
{
    public bool Success { get; set; } = true;
    public string RecurringJobId { get; set; } = null!;
    public string CronExpression { get; set; } = null!;
}

public class RecurringJobListItemResponse
{
    public string RecurringJobId { get; set; } = null!;
    public string CronExpression { get; set; } = null!;
    public string? TimeZoneId { get; set; }
    public string TaskName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
}