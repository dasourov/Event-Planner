using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;
using EventPlanner.Server.Infrastructure.SignalR;

namespace EventPlanner.Server.Features.Comments.DeleteComment;

public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, DeleteCommentResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<CommentHub> _hubContext;

    public DeleteCommentHandler(
        ICommentRepository commentRepository,
        IUserRepository userRepository,
        IHubContext<CommentHub> hubContext
    )
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
        _hubContext = hubContext;
    }

    public async Task<DeleteCommentResponse> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.CommentId);
        if (comment == null)
        {
            throw new Exception("Comment not found");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        // Only author or admin can delete
        if (comment.UserId != request.UserId && user.Role != UserRole.Admin)
        {
            throw new Exception("Not authorized to delete this comment");
        }

        await _commentRepository.DeleteAsync(comment.Id);

        // Broadcast comment deletion
        await _hubContext.Clients.Group(comment.EventId).SendAsync("CommentDeleted", comment.Id, cancellationToken);

        return new DeleteCommentResponse(true);
    }
}
