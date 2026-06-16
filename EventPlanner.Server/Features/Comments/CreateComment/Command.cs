using MediatR;

namespace EventPlanner.Server.Features.Comments.CreateComment;

public record CreateCommentCommand(string EventId, string UserId, string Content, string? ParentCommentId = null) : IRequest<CreateCommentResponse>;
