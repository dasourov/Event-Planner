using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Features.Admin.CreateCategory;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.UnitTests;

public class CreateCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly CreateCategoryHandler _handler;

    public CreateCategoryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new CreateCategoryHandler(_categoryRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenNameIsUniqueAndUserIsAdmin()
    {
        // Arrange
        var adminUser = new User { Id = "admin-id", Username = "admin", Role = UserRole.Admin };
        var command = new CreateCategoryCommand("Technology", "Tech events and meetups", adminUser.Id);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(adminUser.Id))
            .ReturnsAsync(adminUser);

        _categoryRepositoryMock
            .Setup(r => r.GetByNameAsync(command.Name))
            .ReturnsAsync((Category?)null);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(command.Name, response.Name);
        Assert.Equal(command.Description, response.Description);
        _categoryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCategoryNameAlreadyExists()
    {
        // Arrange
        var adminUser = new User { Id = "admin-id", Username = "admin", Role = UserRole.Admin };
        var command = new CreateCategoryCommand("Music", "duplicate", adminUser.Id);
        var existingCategory = new Category { Id = "cat-id", Name = "Music" };

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(adminUser.Id))
            .ReturnsAsync(adminUser);

        _categoryRepositoryMock
            .Setup(r => r.GetByNameAsync(command.Name))
            .ReturnsAsync(existingCategory);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        _categoryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCallerIsNotAdmin()
    {
        // Arrange
        var regularUser = new User { Id = "user-id", Username = "user", Role = UserRole.User };
        var command = new CreateCategoryCommand("Sports", "...", regularUser.Id);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(regularUser.Id))
            .ReturnsAsync(regularUser);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        _categoryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Category>()), Times.Never);
        _categoryRepositoryMock.Verify(r => r.GetByNameAsync(It.IsAny<string>()), Times.Never);
    }
}