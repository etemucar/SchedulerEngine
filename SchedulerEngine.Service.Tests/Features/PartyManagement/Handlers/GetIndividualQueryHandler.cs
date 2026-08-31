using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.TMFCommon;
using SchedulerEngine.Service.Features.Queries;
using SchedulerEngine.Service.Features.Handlers;

namespace SchedulerEngine.Service.Tests.Features.Queries;

public class GetIndividualQueryHandlerTests
{
    private readonly Mock<IRepository<Individual, int>>          _individualRepositoryMock;
    private readonly Mock<ILogger<GetIndividualQueryHandler>>    _loggerMock;
    private readonly GetIndividualQueryHandler                   _handler;

    public GetIndividualQueryHandlerTests()
    {
        _individualRepositoryMock = new Mock<IRepository<Individual, int>>();
        _loggerMock               = new Mock<ILogger<GetIndividualQueryHandler>>();

        _handler = new GetIndividualQueryHandler(
            _individualRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingIndividual_ShouldReturnResponse()
    {
        // Arrange
        var existing = new Individual
        {
            Id            = 1,
            GivenName     = "Ahmet",
            FamilyName    = "Yılmaz",
            Gender        = "Male",
            ValidForStart = DateTime.MinValue,
            ValidForEnd   = DateTime.MaxValue
        };

        _individualRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _handler.Handle(new GetIndividualQuery { Id = 1 }, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.GivenName.Should().Be("Ahmet");
        result.FamilyName.Should().Be("Yılmaz");
        result.Gender.Should().Be("Male");
    }

    [Fact]
    public async Task Handle_NonExistingIndividual_ShouldReturnNull()
    {
        // Arrange
        _individualRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Individual?)null);

        // Act
        var result = await _handler.Handle(new GetIndividualQuery { Id = 99 }, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapValidForCorrectly()
    {
        // Arrange — mock'taki ValidFor değerleriyle assertion tutarlı olmalı
        var start = new DateTime(2024, 1, 1);
        var end   = new DateTime(2025, 12, 31);

        var existing = new Individual
        {
            Id            = 1,
            GivenName     = "Ahmet",
            FamilyName    = "Yılmaz",
            ValidForStart = start,   // assertion'daki değerle aynı
            ValidForEnd   = end
        };

        _individualRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _handler.Handle(new GetIndividualQuery { Id = 1 }, TestContext.Current.CancellationToken);

        // Assert
        result.ValidFor.StartDateTime.Should().Be(start);
        result.ValidFor.EndDateTime.Should().Be(end);
    }
}
