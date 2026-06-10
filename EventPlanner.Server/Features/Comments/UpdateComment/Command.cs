using MediatR;

namespace EventPlanner.Server.Features.Comments.UpdateComment;

public record UpdateCommentCommand(string CommentId, string UserId, string Content) : IRequest<UpdateCommentResponse>;
