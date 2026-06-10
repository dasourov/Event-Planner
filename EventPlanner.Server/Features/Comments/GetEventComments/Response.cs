using System;

namespace EventPlanner.Server.Features.Comments.GetEventComments;

public record GetEventCommentsResponse(
    string Id,
    string EventId,
    string UserId,
    string Username,
    string Content,
    DateTime CreatedAt
);
