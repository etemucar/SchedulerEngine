using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Queries;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;

namespace SchedulerEngine.Service.Features.Handlers;

public class GetIndividualListQueryHandler : IRequestHandler<GetIndividualListQuery, IEnumerable<IndividualResponse>>
{
    private readonly IRepository<Individual, int> _individualRepository;
    private readonly ILogger<GetIndividualListQueryHandler> _logger;

    public GetIndividualListQueryHandler(
        IRepository<Individual, int> individualRepository,
        ILogger<GetIndividualListQueryHandler> logger)
    {
        _individualRepository = individualRepository;
        _logger               = logger;
    }

    public async Task<IEnumerable<IndividualResponse>> Handle(GetIndividualListQuery request, CancellationToken cancellationToken)
    {
        var individuals = await _individualRepository.FindAsync(
            predicate: _ => true,
            orderBy: q => q.OrderBy(x => x.Id),
            ct: cancellationToken);

        return individuals.Select(MapToResponse);
    }

    private static IndividualResponse MapToResponse(Individual individual) => new()
    {
        Id             = individual.Id,
        GivenName      = individual.GivenName,
        FamilyName     = individual.FamilyName,
        MiddleName     = individual.MiddleName,
        Title          = individual.Title,
        Gender         = individual.Gender,
        Nationality    = individual.Nationality,
        BirthDate      = individual.BirthDate,
        PlaceOfBirth   = individual.PlaceOfBirth,
        CountryOfBirth = individual.CountryOfBirth,
        MaritalStatus  = individual.MaritalStatus,
        ValidFor = new TimePeriodResponse
        {
            StartDateTime = individual.ValidForStart,
            EndDateTime   = individual.ValidForEnd
        }
    };
}
