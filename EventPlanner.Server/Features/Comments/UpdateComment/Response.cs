using System;

namespace EventPlanner.Server.Features.Comments.UpdateComment;

public record UpdateCommentResponse(string Id, string Content, DateTime CreatedAt);
