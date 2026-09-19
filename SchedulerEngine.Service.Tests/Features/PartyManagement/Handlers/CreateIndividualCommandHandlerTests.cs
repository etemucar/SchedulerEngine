using Moq;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Data;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Service.Features.Handlers;
using SchedulerEngine.Service.Dtos.Responses;

namespace SchedulerEngine.Service.Tests.Features.Handlers;

public class CreateIndividualCommandHandlerTests
{
    private readonly Mock<IRepository<Party, int>>                 _partyRepositoryMock;
    private readonly Mock<IUnitOfWork>                             _unitOfWorkMock;
    private readonly Mock<IMapper>                                 _mapperMock;
    private readonly Mock<ILogger<CreateIndividualCommandHandler>> _loggerMock;
    private readonly CreateIndividualCommandHandler                _handler;

    public CreateIndividualCommandHandlerTests()
    {
        _partyRepositoryMock = new Mock<IRepository<Party, int>>();
        _unitOfWorkMock      = new Mock<IUnitOfWork>();
        _mapperMock          = new Mock<IMapper>();
        _loggerMock          = new Mock<ILogger<CreateIndividualCommandHandler>>();

        _partyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // DÜZELTME: handler artık IUnitOfWork alıyor — party.Id SaveChanges'ten
        // önce okunursa her zaman 0 dönüyordu (Individual.PartyId FK'sine elle
        // atanıyordu). Artık FK, navigation property (party.Individual = individual)
        // ile kuruluyor ve IRepository<Individual,int> handler'dan tamamen kaldırıldı.
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Gerçek mapping'i taklit ediyoruz — testlerin assert kısımları
        // result.GivenName/FamilyName/... alanlarına doğrudan bakıyor.
        _mapperMock
            .Setup(x => x.Map<IndividualResponse>(It.IsAny<Individual>()))
            .Returns((Individual i) => new IndividualResponse
            {
                Id             = i.Id,
                GivenName      = i.GivenName,
                FamilyName     = i.FamilyName,
                MiddleName     = i.MiddleName,
                Title          = i.Title,
                Gender         = i.Gender,
                Nationality    = i.Nationality,
                BirthDate      = i.BirthDate,
                PlaceOfBirth   = i.PlaceOfBirth,
                CountryOfBirth = i.CountryOfBirth,
                MaritalStatus  = i.MaritalStatus,
                ValidFor = new TimePeriodResponse
                {
                    StartDateTime = i.ValidForStart,
                    EndDateTime   = i.ValidForEnd
                }
            });

        _handler = new CreateIndividualCommandHandler(
            _partyRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePartyAndIndividual()
    {
        // Arrange
        var command = new CreateIndividualCommand
        {
            GivenName  = "Ahmet",
            FamilyName = "Yılmaz",
            Gender     = "Male",
            BirthDate  = new DateTime(1990, 1, 1)
        };

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.GivenName.Should().Be(command.GivenName);
        result.FamilyName.Should().Be(command.FamilyName);
        result.Gender.Should().Be(command.Gender);
        result.BirthDate.Should().Be(command.BirthDate);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallPartyRepositoryOnce()
    {
        // Arrange
        var command = new CreateIndividualCommand { GivenName = "Ahmet", FamilyName = "Yılmaz" };

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _partyRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldLinkIndividualToPartyViaNavigation()
    {
        // DEĞİŞTİ: eskiden "individual.PartyId doğru mu" test ediliyordu (elle FK
        // ataması). Artık FK, navigation property üzerinden kuruluyor ve gerçek
        // .PartyId değeri ancak DB'de SaveChanges gerçekleştiğinde (mock'ta
        // simüle edilmiyor) EF tarafından set edilir. Bu yüzden artık test
        // edilebilir/anlamlı olan şey "navigation doğru kuruldu mu"dur.
        var command = new CreateIndividualCommand { GivenName = "Ahmet", FamilyName = "Yılmaz" };

        Party? capturedParty = null;
        _partyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()))
            .Callback<Party, CancellationToken>((party, _) => capturedParty = party)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        capturedParty.Should().NotBeNull();
        capturedParty!.Individual.Should().NotBeNull();
        capturedParty.Individual!.GivenName.Should().Be("Ahmet");
        capturedParty.Individual.FamilyName.Should().Be("Yılmaz");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSaveChangesAfterAddingParty()
    {
        // YENİ TEST: appUser/individual.Id'nin SaveChanges'ten önce okunması
        // bug'ının düzeltildiğini doğrulayan regresyon testi.
        var command = new CreateIndividualCommand { GivenName = "Ahmet", FamilyName = "Yılmaz" };

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutValidFor_ShouldSetMinMaxDateTime()
    {
        // Arrange — ValidFor gönderilmediğinde handler DateTime.MinValue/MaxValue set etmeli
        var command = new CreateIndividualCommand
        {
            GivenName     = "Ahmet",
            FamilyName    = "Yılmaz",
            ValidForStart = null,
            ValidForEnd   = null
        };

        Party? capturedParty = null;
        _partyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()))
            .Callback<Party, CancellationToken>((party, _) => capturedParty = party)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        capturedParty!.Individual!.ValidForStart.Should().Be(DateTime.MinValue);
        capturedParty.Individual.ValidForEnd.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public async Task Handle_WithValidFor_ShouldMapDatesCorrectly()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate   = new DateTime(2025, 12, 31);

        var command = new CreateIndividualCommand
        {
            GivenName     = "Ahmet",
            FamilyName    = "Yılmaz",
            ValidForStart = startDate,
            ValidForEnd   = endDate
        };

        Party? capturedParty = null;
        _partyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()))
            .Callback<Party, CancellationToken>((party, _) => capturedParty = party)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        capturedParty!.Individual!.ValidForStart.Should().Be(startDate);
        capturedParty.Individual.ValidForEnd.Should().Be(endDate);
    }
}