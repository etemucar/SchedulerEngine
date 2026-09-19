using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Data;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.TMFCommon;
using SchedulerEngine.Service.Features.Commands;

namespace SchedulerEngine.Service.Features.Handlers;

public class CreateIndividualCommandHandler : IRequestHandler<CreateIndividualCommand, IndividualResponse>
{
    private readonly IRepository<Party, int> _partyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateIndividualCommandHandler> _logger;

    public CreateIndividualCommandHandler(
        IRepository<Party, int> partyRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateIndividualCommandHandler> logger)
    {
        _partyRepository = partyRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IndividualResponse> Handle(CreateIndividualCommand request, CancellationToken cancellationToken)
    {
        var individual = new Individual
        {
            GivenName = request.GivenName,
            FamilyName = request.FamilyName,
            MiddleName = request.MiddleName,
            Title = request.Title,
            Gender = request.Gender,
            Nationality = request.Nationality,
            BirthDate = request.BirthDate,
            PlaceOfBirth = request.PlaceOfBirth,
            CountryOfBirth = request.CountryOfBirth,
            MaritalStatus = request.MaritalStatus,
            ValidForStart = request.ValidForStart ?? DateTime.MinValue,
            ValidForEnd = request.ValidForEnd ?? DateTime.MaxValue,
        };

        // FK artık elle .Id okunarak DEĞİL, navigation property ile kuruluyor —
        // party.Id SaveChanges'ten önce 0 olsa bile EF bu referanstan FK'yi
        // kendisi çözer (önceki halde individual.PartyId = party.Id yazılıyordu,
        // bu satır çalıştığı anda party.Id garanti 0'dı — gerçek bug).
        var party = new Party
        {
            Individual = individual
        };

        await _partyRepository.AddAsync(party, cancellationToken);

        // Log ve response'ta gerçek Id'lerin görünmesi için SaveChanges burada,
        // handler içinde MANUEL tetikleniyor — TransactionalBehavior'ın
        // SaveChanges'i next() DÖNDÜKTEN SONRA çağırdığını unutmayın; o ana
        // kadar party.Id/individual.Id hâlâ 0'dır (bkz. Repository.cs: AddAsync
        // sadece change tracker'a ekler, DB'ye yazmaz).
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Individual oluşturuldu. PartyId: {PartyId}, IndividualId: {IndividualId}",
            party.Id, individual.Id);

        return _mapper.Map<IndividualResponse>(individual);
    }
}