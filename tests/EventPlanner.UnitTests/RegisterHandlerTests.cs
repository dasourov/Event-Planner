using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Features.Auth.Register;
using EventPlanner.Server.Infrastructure.Auth;
using EventPlanner.Server.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

using EventPlanner.Server.Common.Errors;

namespace EventPlanner.UnitTests;

public class RegisterHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasher = new PasswordHasher();
        
        var jwtSettings = new JwtSettings
        {
            Secret = "super_secret_key_that_is_at_least_32_characters_long_12345!",
            Issuer = "EventPlanner",
            Audience = "EventPlanner",
            ExpiryMinutes = 60
        };
        var options = Options.Create(jwtSettings);
        _jwtTokenService = new JwtTokenService(options);

        _handler = new RegisterHandler(_userRepositoryMock.Object, _passwordHasher, _jwtTokenService);
    }

    [Fact]
    public async Task Handle_ShouldRegisterUser_WhenEmailAndUsernameAreUnique()
    {
        // Arrange
        var command = new RegisterCommand("newuser", "newuser@example.com", "Password123!");
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(command.Username)).ReturnsAsync((User?)null);

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(command.Username, response.Username);
        Assert.Equal(command.Email, response.Email);
        Assert.NotEmpty(response.Token);
        _userRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterCommand("newuser", "existing@example.com", "Password123!");
        var existingUser = new User { Email = command.Email };
        _userRepositoryMock.Setup(repo => repo.GetByEmailAsync(command.Email)).ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
        _userRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<User>()), Times.Never);
    }
}
