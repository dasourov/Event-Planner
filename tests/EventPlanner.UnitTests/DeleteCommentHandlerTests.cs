using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.AspNetCore.SignalR;
using EventPlanner.Server.Common.Errors;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Features.Comments.DeleteComment;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.UnitTests;

public class DeleteCommentHandlerTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly DeleteCommentHandler _handler;

    private const string EventId = "65f000000000000000000001";
    private const string UserId = "65f000000000000000000002";
    private const string CommentId = "65f000000000000000000003";

    public DeleteCommentHandlerTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        var (hubContext, clientProxy) = CommentHubMocks.Create();
        _clientProxyMock = clientProxy;

        _handler = new DeleteCommentHandler(
            _commentRepositoryMock.Object,
            _userRepositoryMock.Object,
            hubContext.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldCascadeDeleteReplies_WhenParentCommentIsDeleted()
    {
        // Arrange: comment with a reply and a nested reply
        var parent = new Comment { Id = CommentId, EventId = EventId, UserId = UserId };
        var allComments = new List<Comment>
        {
            parent,
            new Comment { Id = "reply1", EventId = EventId, UserId = "other", ParentCommentId = CommentId },
            new Comment { Id = "reply2", EventId = EventId, UserId = "other", ParentCommentId = "reply1" },
            new Comment { Id = "unrelated", EventId = EventId, UserId = "other" }
        };
        _commentRepositoryMock.Setup(repo => repo.GetByIdAsync(CommentId)).ReturnsAsync(parent);
        _commentRepositoryMock.Setup(repo => repo.ListByEventAsync(EventId)).ReturnsAsync(allComments);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(UserId)).ReturnsAsync(new User { Id = UserId, Role = UserRole.User });

        List<string>? deletedIds = null;
        _commentRepositoryMock
            .Setup(repo => repo.DeleteManyAsync(It.IsAny<List<string>>()))
            .Callback<List<string>>(ids => deletedIds = ids)
            .Returns(Task.CompletedTask);

        // Act
        var response = await _handler.Handle(new DeleteCommentCommand(EventId, CommentId, UserId), CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(deletedIds);
        Assert.Equal(new[] { CommentId, "reply1", "reply2" }.OrderBy(x => x), deletedIds!.OrderBy(x => x));
        Assert.DoesNotContain("unrelated", deletedIds!);
        _clientProxyMock.Verify(
            proxy => proxy.SendCoreAsync("CommentDeleted", It.IsAny<object[]>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task Handle_ShouldThrowForbidden_WhenUserIsNeitherAuthorNorAdmin()
    {
        // Arrange
        var comment = new Comment { Id = CommentId, EventId = EventId, UserId = "author" };
        _commentRepositoryMock.Setup(repo => repo.GetByIdAsync(CommentId)).ReturnsAsync(comment);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(UserId)).ReturnsAsync(new User { Id = UserId, Role = UserRole.User });

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => _handler.Handle(new DeleteCommentCommand(EventId, CommentId, UserId), CancellationToken.None));
        _commentRepositoryMock.Verify(repo => repo.DeleteManyAsync(It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAllowAdmin_ToDeleteAnotherUsersComment()
    {
        // Arrange
        var comment = new Comment { Id = CommentId, EventId = EventId, UserId = "author" };
        _commentRepositoryMock.Setup(repo => repo.GetByIdAsync(CommentId)).ReturnsAsync(comment);
        _commentRepositoryMock.Setup(repo => repo.ListByEventAsync(EventId)).ReturnsAsync(new List<Comment> { comment });
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(UserId)).ReturnsAsync(new User { Id = UserId, Role = UserRole.Admin });

        // Act
        var response = await _handler.Handle(new DeleteCommentCommand(EventId, CommentId, UserId), CancellationToken.None);

        // Assert
        Assert.True(response.Success);
        _commentRepositoryMock.Verify(repo => repo.DeleteManyAsync(It.Is<List<string>>(ids => ids.Count == 1 && ids[0] == CommentId)), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenCommentDoesNotExist()
    {
        // Arrange
        _commentRepositoryMock.Setup(repo => repo.GetByIdAsync(CommentId)).ReturnsAsync((Comment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _handler.Handle(new DeleteCommentCommand(EventId, CommentId, UserId), CancellationToken.None));
    }
}
