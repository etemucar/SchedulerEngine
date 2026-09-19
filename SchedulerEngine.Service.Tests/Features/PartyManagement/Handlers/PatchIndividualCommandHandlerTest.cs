using Moq;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Exceptions;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.TMFCommon;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Service.Features.Handlers;
using SchedulerEngine.Service.Dtos.Requests;

namespace SchedulerEngine.Service.Tests.Features.Handlers;

public class PatchIndividualCommandHandlerTests
{
    private readonly Mock<IRepository<Individual, int>>            _individualRepositoryMock;
    private readonly Mock<IMapper>                                 _mapperMock;
    private readonly Mock<ILogger<PatchIndividualCommandHandler>>  _loggerMock;
    private readonly PatchIndividualCommandHandler                 _handler;

    public PatchIndividualCommandHandlerTests()
    {
        _individualRepositoryMock = new Mock<IRepository<Individual, int>>();
        _mapperMock               = new Mock<IMapper>();
        _loggerMock               = new Mock<ILogger<PatchIndividualCommandHandler>>();

        // Handler'ı AutoMapper profilinden izole tutmak için manuel mapping — sadece
        // testlerin assert ettiği alanları taşıyor.
        _mapperMock
            .Setup(x => x.Map<IndividualResponse>(It.IsAny<Individual>()))
            .Returns((Individual src) => new IndividualResponse
            {
                Id         = src.Id,
                GivenName  = src.GivenName,
                FamilyName = src.FamilyName,
                ValidFor   = new TimePeriodResponse
                {
                    StartDateTime = src.ValidForStart,
                    EndDateTime   = src.ValidForEnd
                }
            });

        _handler = new PatchIndividualCommandHandler(
            _individualRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingIndividual_ShouldUpdateAndReturnResponse()
    {
        // Arrange
        var existing = new Individual
        {
            Id         = 1,
            GivenName  = "Ahmet",
            FamilyName = "Yılmaz",
            ValidForStart = DateTime.MinValue, 
            ValidForEnd = DateTime.MaxValue

        };

        var command = new PatchIndividualCommand
        {
            Id        = 1,
            GivenName = "Mehmet"
        };

        _individualRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _individualRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Individual>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.GivenName.Should().Be("Mehmet");
        result.FamilyName.Should().Be("Yılmaz"); // değişmemeli
    }

    [Fact]
    public async Task Handle_NonExistingIndividual_ShouldThrowNotFoundException()
    {
        // Arrange — handler artık null dönmüyor, NotFoundException fırlatıyor
        // (bkz. PatchIndividualCommandHandler.cs satır 34)
        var command = new PatchIndividualCommand { Id = 99 };

        _individualRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Individual?)null);

        // Act
        Func<Task> act = () => _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }

    [Fact]
    public async Task Handle_WithValidFor_ShouldUpdateDates()
    {
        // Arrange
        var existing = new Individual
        {
            Id         = 1,
            GivenName  = "Ahmet",
            FamilyName = "Yılmaz",
            ValidForStart = DateTime.MinValue, 
            ValidForEnd = DateTime.MaxValue
        };

        var newStart = new DateTime(2024, 1, 1);
        var newEnd   = new DateTime(2025, 12, 31);

        var command = new PatchIndividualCommand
        {
            Id       = 1,
            ValidForStart = newStart, 
            ValidForEnd = newEnd
        };

        _individualRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _individualRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Individual>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        result.ValidFor.StartDateTime.Should().Be(newStart);
        result.ValidFor.EndDateTime.Should().Be(newEnd);
    }

    [Fact]
    public async Task Handle_ShouldCallUpdateRepositoryOnce()
    {
        // Arrange
        var existing = new Individual
        {
            Id         = 1,
            GivenName  = "Ahmet",
            FamilyName = "Yılmaz",
            ValidForStart = DateTime.MinValue, 
            ValidForEnd = DateTime.MaxValue
        };

        var command = new PatchIndividualCommand { Id = 1, GivenName = "Mehmet" };

        _individualRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _individualRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Individual>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        _individualRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Individual>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}