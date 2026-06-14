using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.AspNetCore.SignalR;
using EventPlanner.Server.Common.Errors;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Features.Comments.UpdateComment;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.UnitTests;

public class UpdateCommentHandlerTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly UpdateCommentHandler _handler;

    private const string EventId = "65f000000000000000000001";
    private const string UserId = "65f000000000000000000002";
    private const string CommentId = "65f000000000000000000003";

    public UpdateCommentHandlerTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        var (hubContext, clientProxy) = CommentHubMocks.Create();
        _clientProxyMock = clientProxy;
        _handler = new UpdateCommentHandler(_commentRepositoryMock.Object, hubContext.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateContent_WhenUserIsAuthor()
    {
        // Arrange
        var comment = new Comment { Id = CommentId, EventId = EventId, UserId = UserId, Content = "old" };
        _commentRepositoryMock.Setup(repo => repo.GetByIdAsync(CommentId)).ReturnsAsync(comment);

        // Act
        var response = await _handler.Handle(new UpdateCommentCommand(EventId, CommentId, UserId, "new"), CancellationToken.None);

        // Assert
        Assert.Equal("new", response.Content);
        _commentRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Comment>(c => c.Content == "new")), Times.Once);
        _clientProxyMock.Verify(
            proxy => proxy.SendCoreAsync("CommentUpdated", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowForbidden_WhenUserIsNotAuthor()
    {
        // Arrange
        var comment = new Comment { Id = CommentId, EventId = EventId, UserId = "someone-else", Content = "old" };
        _commentRepositoryMock.Setup(repo => repo.GetByIdAsync(CommentId)).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _handler.Handle(new UpdateCommentCommand(EventId, CommentId, UserId, "new"), CancellationToken.None));
        _commentRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCommentBelongsToAnotherEvent()
    {
        // Arrange
        var comment = new Comment { Id = CommentId, EventId = "65f0000000000000000000ee", UserId = UserId };
        _commentRepositoryMock.Setup(repo => repo.GetByIdAsync(CommentId)).ReturnsAsync(comment);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new UpdateCommentCommand(EventId, CommentId, UserId, "new"), CancellationToken.None));
    }
}
