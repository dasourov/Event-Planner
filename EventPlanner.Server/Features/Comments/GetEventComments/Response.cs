using System;
using System.Collections.Generic;

namespace EventPlanner.Server.Features.Comments.GetEventComments;

public record GetEventCommentsResponse(
    string Id,
    string EventId,
    string UserId,
    string Username,
    string Content,
    string? ParentCommentId,
    DateTime CreatedAt,
    List<GetEventCommentsResponse> Replies
);
