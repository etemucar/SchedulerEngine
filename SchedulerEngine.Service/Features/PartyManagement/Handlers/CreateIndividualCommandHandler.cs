using MediatR;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.TMFCommon;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Handlers;

public class CreateIndividualCommandHandler : IRequestHandler<CreateIndividualCommand, IndividualResponse>
{
    private readonly IRepository<Party, int>                        _partyRepository;
    private readonly IRepository<Individual, int>                   _individualRepository;
    private readonly ILogger<CreateIndividualCommandHandler>        _logger;

    public CreateIndividualCommandHandler(
        IRepository<Party, int>                  partyRepository,
        IRepository<Individual, int>             individualRepository,
        ILogger<CreateIndividualCommandHandler>  logger)
    {
        _partyRepository      = partyRepository;
        _individualRepository = individualRepository;
        _logger               = logger;
    }

    public async Task<IndividualResponse> Handle(CreateIndividualCommand request, CancellationToken cancellationToken)
    {
        // 1. Party oluştur (abstract container)
        var party = new Party();
        await _partyRepository.AddAsync(party, cancellationToken);

        // 2. Individual oluştur
        var individual = new Individual
        {
            PartyId        = party.Id,
            GivenName      = request.GivenName,
            FamilyName     = request.FamilyName,
            MiddleName     = request.MiddleName,
            Title          = request.Title,
            Gender         = request.Gender,
            Nationality    = request.Nationality,
            BirthDate      = request.BirthDate,
            PlaceOfBirth   = request.PlaceOfBirth,
            CountryOfBirth = request.CountryOfBirth,
            MaritalStatus  = request.MaritalStatus,
            // null gelirse Min/Max ile aç — kural 4
            ValidForStart  = request.ValidForStart ?? DateTime.MinValue,
            ValidForEnd    = request.ValidForEnd   ?? DateTime.MaxValue,
        };

        await _individualRepository.AddAsync(individual, cancellationToken);

        _logger.LogInformation(
            "Individual oluşturuldu. PartyId: {PartyId}, IndividualId: {IndividualId}",
            party.Id, individual.Id);

        return MapToResponse(individual);
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
