using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Service.Features.Handlers;
using SchedulerEngine.Service.Dtos.Requests;

namespace SchedulerEngine.Service.Tests.Features.Handlers;

public class CreatePartyRoleCommandHandlerTests
{
    private readonly Mock<IRepository<PartyRole, int>>            _partyRoleRepositoryMock;
    private readonly Mock<ILogger<CreatePartyRoleCommandHandler>> _loggerMock;
    private readonly CreatePartyRoleCommandHandler                _handler;

    public CreatePartyRoleCommandHandlerTests()
    {
        _partyRoleRepositoryMock = new Mock<IRepository<PartyRole, int>>();
        _loggerMock              = new Mock<ILogger<CreatePartyRoleCommandHandler>>();

        _handler = new CreatePartyRoleCommandHandler(
            _partyRoleRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreatePartyRoleAndReturnResponse()
    {
        // Arrange
        var command = new CreatePartyRoleCommand
        {
            PartyId         = 1,
            PartyRoleTypeId = 1
        };

        _partyRoleRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PartyRole>(), It.IsAny<CancellationToken>()))
            .Callback<PartyRole, CancellationToken>((pr, _) => pr.Id = 10)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.PartyId.Should().Be(command.PartyId);
        result.PartyRoleTypeId.Should().Be(command.PartyRoleTypeId);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallRepositoryOnce()
    {
        // Arrange
        var command = new CreatePartyRoleCommand
        {
            PartyId         = 1,
            PartyRoleTypeId = 1
        };

        _partyRoleRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PartyRole>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _partyRoleRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<PartyRole>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutValidFor_ShouldSetMinMaxDateTime()
    {
        // Arrange — ValidFor gönderilmediğinde handler DateTime.MinValue/MaxValue set etmeli
        var command = new CreatePartyRoleCommand
        {
            PartyId         = 1,
            PartyRoleTypeId = 1,
            ValidForStart   = null,
            ValidForEnd     = null
        };

        PartyRole capturedPartyRole = null!;
        _partyRoleRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PartyRole>(), It.IsAny<CancellationToken>()))
            .Callback<PartyRole, CancellationToken>((pr, _) => capturedPartyRole = pr)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert — DateTime? null değil, DateTime.MinValue/MaxValue olmalı
        capturedPartyRole.ValidForStart.Should().NotBeNull();
        capturedPartyRole.ValidForStart.Should().Be(DateTime.MinValue);
        capturedPartyRole.ValidForEnd.Should().NotBeNull();
        capturedPartyRole.ValidForEnd.Should().Be(DateTime.MaxValue);
    }

    [Fact]
    public async Task Handle_WithValidFor_ShouldMapDatesCorrectly()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate   = new DateTime(2025, 12, 31);

        var command = new CreatePartyRoleCommand
        {
            PartyId         = 1,
            PartyRoleTypeId = 1,
            ValidForStart   = startDate,
            ValidForEnd     = endDate
        };

        PartyRole capturedPartyRole = null!;
        _partyRoleRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<PartyRole>(), It.IsAny<CancellationToken>()))
            .Callback<PartyRole, CancellationToken>((pr, _) => capturedPartyRole = pr)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        capturedPartyRole.ValidForStart.Should().Be(startDate);
        capturedPartyRole.ValidForEnd.Should().Be(endDate);
    }
}
