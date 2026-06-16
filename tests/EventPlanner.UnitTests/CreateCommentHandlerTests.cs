using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using EventPlanner.Server.Common.Errors;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Features.Comments.CreateComment;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.UnitTests;

public class CreateCommentHandlerTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly CreateCommentHandler _handler;

    private const string EventId = "65f000000000000000000001";
    private const string UserId = "65f000000000000000000002";
    private const string ParentId = "65f000000000000000000003";

    public CreateCommentHandlerTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        var (hubContext, clientProxy) = CommentHubMocks.Create();
        _clientProxyMock = clientProxy;

        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(EventId)).ReturnsAsync(new Event { Id = EventId });
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(UserId)).ReturnsAsync(new User { Id = UserId, Username = "tester" });
        _commentRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Comment>()))
            .Callback<Comment>(c => c.Id = ObjectId.GenerateNewId().ToString())
            .Returns(Task.CompletedTask);

        _handler = new CreateCommentHandler(
            _commentRepositoryMock.Object,
            _eventRepositoryMock.Object,
            _userRepositoryMock.Object,
            hubContext.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCreateTopLevelComment_AndBroadcastIt()
    {
        // Act
        var response = await _handler.Handle(new CreateCommentCommand(EventId, UserId, "Hello!"), CancellationToken.None);

        // Assert
        Assert.Equal("Hello!", response.Content);
        Assert.Equal("tester", response.Username);
        Assert.Null(response.ParentCommentId);
        _commentRepositoryMock.Verify(repo => repo.CreateAsync(It.Is<Comment>(c => c.ParentCommentId == null)), Times.Once);
        _clientProxyMock.Verify(
            proxy => proxy.SendCoreAsync("CommentCreated", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateNestedReply_WhenParentBelongsToSameEvent()
    {
        // Arrange
        _commentRepositoryMock.Setup(repo => repo.GetByIdAsync(ParentId))
            .ReturnsAsync(new Comment { Id = ParentId, EventId = EventId });

        // Act
        var response = await _handler.Handle(new CreateCommentCommand(EventId, UserId, "A reply", ParentId), CancellationToken.None);

        // Assert
        Assert.Equal(ParentId, response.ParentCommentId);
        _commentRepositoryMock.Verify(repo => repo.CreateAsync(It.Is<Comment>(c => c.ParentCommentId == ParentId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenParentCommentDoesNotExist()
    {
        // Arrange
        _commentRepositoryMock.Setup(repo => repo.GetByIdAsync(ParentId)).ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new CreateCommentCommand(EventId, UserId, "A reply", ParentId), CancellationToken.None));
        _commentRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenParentCommentBelongsToAnotherEvent()
    {
        // Arrange
        _commentRepositoryMock.Setup(repo => repo.GetByIdAsync(ParentId))
            .ReturnsAsync(new Comment { Id = ParentId, EventId = "65f0000000000000000000ee" });

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => _handler.Handle(new CreateCommentCommand(EventId, UserId, "A reply", ParentId), CancellationToken.None));
        _commentRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenEventDoesNotExist()
    {
        // Arrange
        _eventRepositoryMock.Setup(repo => repo.GetByIdAsync(EventId)).ReturnsAsync((Event?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new CreateCommentCommand(EventId, UserId, "Hello!"), CancellationToken.None));
    }
}
