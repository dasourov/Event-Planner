using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Features.Events.CreateEvent;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.UnitTests;

public class CreateEventHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CreateEventHandler _handler;

    public CreateEventHandlerTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _handler = new CreateEventHandler(_eventRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateEvent_WhenCategoryExists()
    {
        // Arrange
        var categoryId = "507f1f77bcf86cd799439011";
        var command = new CreateEventCommand(
            "Tech Conference",
            "A conference about new tech",
            "New York",
            40.7128,
            -74.0060,
            DateTime.UtcNow.AddDays(5),
            categoryId,
            100,
            "user123"
        );
        var category = new Category { Id = categoryId, Name = "Tech" };
        _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(command.CategoryId)).ReturnsAsync(category);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(command.Title, response.Title);
        Assert.Equal(command.CategoryId, response.CategoryId);
        Assert.Equal(command.OrganizerId, response.OrganizerId);
        Assert.Equal("Draft", response.Status);
        _eventRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldResolveToGeneralCategory_WhenCategoryDoesNotExist()
    {
        // Arrange
        var command = new CreateEventCommand(
            "Tech Conference",
            "A conference about new tech",
            "New York",
            40.7128,
            -74.0060,
            DateTime.UtcNow.AddDays(5),
            "invalid_cat",
            100,
            "user123"
        );
        _categoryRepositoryMock.Setup(repo => repo.GetByNameAsync("General")).ReturnsAsync((Category?)null);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(command.Title, response.Title);
        Assert.Equal(command.OrganizerId, response.OrganizerId);
        Assert.Equal("Draft", response.Status);
        _categoryRepositoryMock.Verify(repo => repo.CreateAsync(It.Is<Category>(c => c.Name == "General")), Times.Once);
        _eventRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Event>()), Times.Once);
    }
}
