using MediatR;

namespace EventPlanner.Server.Features.Comments.UpdateComment;

public record UpdateCommentCommand(string EventId, string CommentId, string UserId, string Content) : IRequest<UpdateCommentResponse>;
