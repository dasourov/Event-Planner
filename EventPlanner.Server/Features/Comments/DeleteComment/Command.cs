using MediatR;

namespace EventPlanner.Server.Features.Comments.DeleteComment;

public record DeleteCommentCommand(string EventId, string CommentId, string UserId) : IRequest<DeleteCommentResponse>;
