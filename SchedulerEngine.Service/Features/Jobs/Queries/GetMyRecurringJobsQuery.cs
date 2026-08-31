using MediatR;
using SchedulerEngine.Service.Dtos.Responses;

namespace SchedulerEngine.Service.Features.Queries;

/// <summary>
/// Çağıran servisin (X-Api-Key ile doğrulanan) kendi kaydettirdiği recurring
/// job'ları listeler - başka bir servisin job'larını görmez (CallerCredentialId
/// ile filtreleniyor, tıpkı Add/Remove'daki sahiplik izolasyonu gibi).
/// </summary>
public record GetMyRecurringJobsQuery(Guid CallerCredentialId, bool IncludeRemoved = false)
    : IRequest<List<RecurringJobListItem>>;