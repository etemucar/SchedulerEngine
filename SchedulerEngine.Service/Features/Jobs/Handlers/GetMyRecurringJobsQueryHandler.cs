using MediatR;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Queries;

namespace SchedulerEngine.Service.Features.Handlers;

public class GetMyRecurringJobsQueryHandler : IRequestHandler<GetMyRecurringJobsQuery, List<RecurringJobListItem>>
{
    private readonly IRepository<ServiceRecurringJob, int> _serviceRecurringJobRepository;

    public GetMyRecurringJobsQueryHandler(IRepository<ServiceRecurringJob, int> serviceRecurringJobRepository)
    {
        _serviceRecurringJobRepository = serviceRecurringJobRepository;
    }

    public async Task<List<RecurringJobListItem>> Handle(GetMyRecurringJobsQuery request, CancellationToken ct)
    {
        var items = await _serviceRecurringJobRepository.FindSelectAsync(
            predicate: x => x.CallerCredentialId == request.CallerCredentialId
                && (request.IncludeRemoved || x.IsActive),
            selector: x => new RecurringJobListItem
            {
                RecurringJobId = x.RecurringJobId,
                CronExpression = x.CronExpression,
                TimeZoneId     = x.TimeZoneId,
                TaskName       = x.TaskName,
                IsActive       = x.IsActive,
                CreatedAt      = x.CreatedAt,
                UpdatedAt      = x.UpdatedAt,
                RemovedAt      = x.RemovedAt
            },
            ct: ct);

        return items.ToList();
    }
}