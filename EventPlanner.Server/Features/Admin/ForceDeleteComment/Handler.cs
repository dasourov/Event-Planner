using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;
using EventPlanner.Server.Infrastructure.SignalR;
using EventPlanner.Server.Common.Errors;

namespace EventPlanner.Server.Features.Admin.ForceDeleteComment;

public class ForceDeleteCommentHandler : IRequestHandler<ForceDeleteCommentCommand, ForceDeleteCommentResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<CommentHub> _hubContext;

    public ForceDeleteCommentHandler(
        ICommentRepository commentRepository,
        IUserRepository userRepository,
        IHubContext<CommentHub> hubContext
    )
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _hubContext = hubContext;
    }

    public async Task<ForceDeleteCommentResponse> Handle(ForceDeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null || user.Role != UserRole.Admin)
        {
            throw new ForbiddenException("Only admins can force delete comments.");
        }

        var comment = await _commentRepository.GetByIdAsync(request.Id);
        if (comment == null)
        {
            throw new NotFoundException("Comment not found.");
        }

        await _commentRepository.DeleteAsync(request.Id);

        // Broadcast comment deletion via SignalR
        await _hubContext.Clients.Group(comment.EventId).SendAsync("CommentDeleted", comment.Id, cancellationToken);

        return new ForceDeleteCommentResponse(true);
    }
}
