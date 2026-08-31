using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Service.Features.Handlers;
using SchedulerEngine.Service.Dtos.Requests;

namespace SchedulerEngine.Service.Tests.Features.Handlers;

public class CreateOrganizationCommandHandlerTests
{
    private readonly Mock<IRepository<Party, int>>                     _partyRepositoryMock;
    private readonly Mock<IRepository<Organization, int>>              _organizationRepositoryMock;
    private readonly Mock<ILogger<CreateOrganizationCommandHandler>>   _loggerMock;
    private readonly CreateOrganizationCommandHandler                  _handler;

    public CreateOrganizationCommandHandlerTests()
    {
        _partyRepositoryMock        = new Mock<IRepository<Party, int>>();
        _organizationRepositoryMock = new Mock<IRepository<Organization, int>>();
        _loggerMock                 = new Mock<ILogger<CreateOrganizationCommandHandler>>();

        _handler = new CreateOrganizationCommandHandler(
            _partyRepositoryMock.Object,
            _organizationRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePartyAndOrganization()
    {
        // Arrange
        var command = new CreateOrganizationCommand
        {
            Name      = "Test A.Ş.",
            TaxOffice = "Kadıköy",
            TaxNumber = 1234567890
        };

        _partyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()))
            .Callback<Party, CancellationToken>((party, _) => party.Id = 1)
            .Returns(Task.CompletedTask);

        _organizationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
            .Callback<Organization, CancellationToken>((org, _) => org.Id = 10)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.Name.Should().Be(command.Name);
        result.TaxOffice.Should().Be(command.TaxOffice);
        result.TaxNumber.Should().Be(command.TaxNumber);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallPartyRepositoryOnce()
    {
        // Arrange
        var command = new CreateOrganizationCommand
        {
            Name      = "Test A.Ş.",
            TaxOffice = "Kadıköy",
            TaxNumber = 1234567890
        };

        _partyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _organizationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _partyRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOrganizationWithCorrectPartyId()
    {
        // Arrange
        var command = new CreateOrganizationCommand
        {
            Name      = "Test A.Ş.",
            TaxOffice = "Kadıköy",
            TaxNumber = 1234567890
        };

        _partyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()))
            .Callback<Party, CancellationToken>((party, _) => party.Id = 42)
            .Returns(Task.CompletedTask);

        Organization capturedOrganization = null!;
        _organizationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
            .Callback<Organization, CancellationToken>((org, _) => capturedOrganization = org)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        capturedOrganization.Should().NotBeNull();
        capturedOrganization.PartyId.Should().Be(42);
    }

    [Fact]
    public async Task Handle_WithoutValidFor_ShouldSetMinMaxDateTime()
    {
        // Arrange — ValidFor gönderilmediğinde handler DateTime.MinValue/MaxValue set etmeli
        var command = new CreateOrganizationCommand
        {
            Name          = "Test A.Ş.",
            TaxOffice     = "Kadıköy",
            TaxNumber     = 1234567890,
            ValidForStart = null,
            ValidForEnd   = null
        };

        _partyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Organization capturedOrganization = null!;
        _organizationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
            .Callback<Organization, CancellationToken>((org, _) => capturedOrganization = org)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert — DateTime? null değil, DateTime.MinValue/MaxValue olmalı
        capturedOrganization.ValidForStart.Should().NotBeNull();
        capturedOrganization.ValidForStart.Should().Be(DateTime.MinValue);
        capturedOrganization.ValidForEnd.Should().NotBeNull();
        capturedOrganization.ValidForEnd.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public async Task Handle_WithValidFor_ShouldMapDatesCorrectly()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate   = new DateTime(2025, 12, 31);

        var command = new CreateOrganizationCommand
        {
            Name          = "Test A.Ş.",
            TaxOffice     = "Kadıköy",
            TaxNumber     = 1234567890,
            ValidForStart = startDate,
            ValidForEnd   = endDate
        };

        _partyRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Party>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Organization capturedOrganization = null!;
        _organizationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Organization>(), It.IsAny<CancellationToken>()))
            .Callback<Organization, CancellationToken>((org, _) => capturedOrganization = org)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        capturedOrganization.ValidForStart.Should().Be(startDate);
        capturedOrganization.ValidForEnd.Should().Be(endDate);
    }
}
