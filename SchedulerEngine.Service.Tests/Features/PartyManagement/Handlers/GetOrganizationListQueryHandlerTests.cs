using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using SchedulerEngine.Core.Repository;
using SchedulerEngine.Core.Model;
using SchedulerEngine.Core.TMFCommon;
using SchedulerEngine.Service.Features.Queries;
using SchedulerEngine.Service.Features.Handlers;

namespace SchedulerEngine.Service.Tests.Features.Queries;

public class GetOrganizationListQueryHandlerTests
{
    private readonly Mock<IRepository<Organization, int>> _organizationRepositoryMock;
    private readonly Mock<ILogger<GetOrganizationListQueryHandler>> _loggerMock;
    private readonly GetOrganizationListQueryHandler _handler;

    public GetOrganizationListQueryHandlerTests()
    {
        _organizationRepositoryMock = new Mock<IRepository<Organization, int>>();
        _loggerMock                 = new Mock<ILogger<GetOrganizationListQueryHandler>>();

        _handler = new GetOrganizationListQueryHandler(
            _organizationRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllOrganizations()
    {
        // Arrange
        var organizations = new List<Organization>
        {
            new() { Id = 1, Name = "Test A.Ş.",   TaxNumber = 1234567890, ValidForStart = DateTime.MinValue, ValidForEnd = DateTime.MaxValue},
            new() { Id = 2, Name = "Örnek Ltd.",  TaxNumber = 9876543210, ValidForStart = DateTime.MinValue, ValidForEnd = DateTime.MaxValue}
        };

        _organizationRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<Func<IQueryable<Organization>, IOrderedQueryable<Organization>>>(),
                It.IsAny<Func<IQueryable<Organization>, IQueryable<Organization>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(organizations);

        // Act
        var result = await _handler.Handle(new GetOrganizationListQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Test A.Ş.");
    }

    [Fact]
    public async Task Handle_EmptyList_ShouldReturnEmptyCollection()
    {
        // Arrange
        _organizationRepositoryMock
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Organization, bool>>>(),
                It.IsAny<Func<IQueryable<Organization>, IOrderedQueryable<Organization>>>(),
                It.IsAny<Func<IQueryable<Organization>, IQueryable<Organization>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Organization>());

        // Act
        var result = await _handler.Handle(new GetOrganizationListQuery(), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEmpty();
    }
}
