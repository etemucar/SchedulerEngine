namespace SchedulerEngine.Service.Dtos.Responses;

public class EnqueueJobResult
{
    public string JobId { get; set; } = null!;
}

public class ScheduleJobResult
{
    public string JobId { get; set; } = null!;
}

public class RecurringJobResult
{
    public string RecurringJobId { get; set; } = null!;
    public string CronExpression { get; set; } = null!;
}