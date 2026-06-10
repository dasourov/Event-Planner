using MediatR;

namespace EventPlanner.Server.Features.Admin.ForceDeleteComment;

public record ForceDeleteCommentCommand(string Id, string UserId) : IRequest<ForceDeleteCommentResponse>;
