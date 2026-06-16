using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Features.Comments.GetEventComments;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.UnitTests;

public class GetEventCommentsHandlerTests
{
    private readonly Mock<ICommentRepository> _commentRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetEventCommentsHandler _handler;

    private const string EventId = "65f000000000000000000001";
    private const string UserId = "65f000000000000000000002";

    public GetEventCommentsHandlerTests()
    {
        _commentRepositoryMock = new Mock<ICommentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _userRepositoryMock
            .Setup(repo => repo.GetByIdsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync(new List<User> { new User { Id = UserId, Username = "tester" } });

        _handler = new GetEventCommentsHandler(_commentRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldBuildNestedTree_WithRepliesUnderTheirParents()
    {
        // Arrange: parent -> reply -> nested reply, plus a second top-level comment
        var now = DateTime.UtcNow;
        var comments = new List<Comment>
        {
            new Comment { Id = "c1", EventId = EventId, UserId = UserId, Content = "parent", CreatedAt = now.AddMinutes(-10) },
            new Comment { Id = "c2", EventId = EventId, UserId = UserId, Content = "reply", ParentCommentId = "c1", CreatedAt = now.AddMinutes(-5) },
            new Comment { Id = "c3", EventId = EventId, UserId = UserId, Content = "nested reply", ParentCommentId = "c2", CreatedAt = now.AddMinutes(-1) },
            new Comment { Id = "c4", EventId = EventId, UserId = UserId, Content = "another top-level", CreatedAt = now }
        };
        _commentRepositoryMock.Setup(repo => repo.ListByEventAsync(EventId)).ReturnsAsync(comments);

        // Act
        var response = await _handler.Handle(new GetEventCommentsQuery(EventId), CancellationToken.None);

        // Assert: two top-level comments, newest first
        Assert.Equal(2, response.Count);
        Assert.Equal("c4", response[0].Id);
        Assert.Equal("c1", response[1].Id);

        // Reply chain is nested
        var parent = response[1];
        Assert.Single(parent.Replies);
        Assert.Equal("c2", parent.Replies[0].Id);
        Assert.Single(parent.Replies[0].Replies);
        Assert.Equal("c3", parent.Replies[0].Replies[0].Id);

        // Usernames are resolved with a single batch lookup
        Assert.Equal("tester", parent.Username);
        _userRepositoryMock.Verify(repo => repo.GetByIdsAsync(It.IsAny<List<string>>()), Times.Once);
        _userRepositoryMock.Verify(repo => repo.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldTreatReplyAsTopLevel_WhenItsParentIsMissing()
    {
        // Arrange: reply whose parent was force-deleted
        var comments = new List<Comment>
        {
            new Comment { Id = "c2", EventId = EventId, UserId = UserId, Content = "orphan reply", ParentCommentId = "gone", CreatedAt = DateTime.UtcNow }
        };
        _commentRepositoryMock.Setup(repo => repo.ListByEventAsync(EventId)).ReturnsAsync(comments);

        // Act
        var response = await _handler.Handle(new GetEventCommentsQuery(EventId), CancellationToken.None);

        // Assert
        Assert.Single(response);
        Assert.Equal("c2", response[0].Id);
    }
}
