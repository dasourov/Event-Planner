using MediatR;

namespace EventPlanner.Server.Features.Comments.CreateComment;

public record CreateCommentCommand(string EventId, string UserId, string Content) : IRequest<CreateCommentResponse>;
