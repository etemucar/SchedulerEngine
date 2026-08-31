using MediatR;
using SchedulerEngine.Service.Dtos.Responses;
 
namespace SchedulerEngine.Service.Features.Queries;

public class GetIndividualQuery : IRequest<IndividualResponse>
{
    public int Id { get; set; }
}

public class GetIndividualListQuery : IRequest<IEnumerable<IndividualResponse>>
{
    public int    Offset { get; set; } = 0;
    public int    Limit  { get; set; } = 20;
    public string? Fields { get; set; } = null!;
}
