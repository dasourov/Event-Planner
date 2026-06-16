using System;

namespace EventPlanner.Server.Features.Comments.CreateComment;

public record CreateCommentResponse(
    string Id,
    string EventId,
    string UserId,
    string Username,
    string Content,
    string? ParentCommentId,
    DateTime CreatedAt
);
