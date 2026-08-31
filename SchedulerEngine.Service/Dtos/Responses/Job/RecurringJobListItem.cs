namespace SchedulerEngine.Service.Dtos.Responses;

public class RecurringJobListItem
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