using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.TMFCommon;
using SchedulerEngine.Service.Features.Queries;
using SchedulerEngine.Service.Features.Handlers;

namespace SchedulerEngine.Service.Tests.Features.Queries;

public class GetOrganizationQueryHandlerTests
{
    private readonly Mock<IRepository<Organization, int>> _organizationRepositoryMock;
    private readonly Mock<ILogger<GetOrganizationQueryHandler>> _loggerMock;
    private readonly GetOrganizationQueryHandler _handler;

    public GetOrganizationQueryHandlerTests()
    {
        _organizationRepositoryMock = new Mock<IRepository<Organization, int>>();
        _loggerMock                 = new Mock<ILogger<GetOrganizationQueryHandler>>();

        _handler = new GetOrganizationQueryHandler(
            _organizationRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingOrganization_ShouldReturnResponse()
    {
        // Arrange
        var existing = new Organization
        {
            Id        = 1,
            Name      = "Test A.Ş.",
            TaxOffice = "Kadıköy",
            TaxNumber = 1234567890,
            ValidForStart = DateTime.MinValue, 
            ValidForEnd = DateTime.MaxValue

        };

        _organizationRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _handler.Handle(new GetOrganizationQuery { Id = 1 }, TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test A.Ş.");
        result.TaxOffice.Should().Be("Kadıköy");
        result.TaxNumber.Should().Be(1234567890);
    }

    [Fact]
    public async Task Handle_NonExistingOrganization_ShouldReturnNull()
    {
        // Arrange
        _organizationRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Organization?)null);

        // Act
        var result = await _handler.Handle(new GetOrganizationQuery { Id = 99 }, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapValidForCorrectly()
    {
        // Arrange
        var start = new DateTime(2024, 1, 1);
        var end   = new DateTime(2025, 12, 31);

        var existing = new Organization
        {
            Id        = 1,
            Name      = "Test A.Ş.",
            TaxNumber = 1234567890,
            ValidForStart = start, 
            ValidForEnd = end
        };

        _organizationRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _handler.Handle(new GetOrganizationQuery { Id = 1 }, TestContext.Current.CancellationToken);

        // Assert
        result.ValidFor.StartDateTime.Should().Be(start);
        result.ValidFor.EndDateTime.Should().Be(end);
    }
}
