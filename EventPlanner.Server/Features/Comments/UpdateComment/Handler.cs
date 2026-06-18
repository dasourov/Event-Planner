using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using EventPlanner.Server.Common.Errors;
using EventPlanner.Server.Infrastructure.Repositories;
using EventPlanner.Server.Infrastructure.SignalR;

namespace EventPlanner.Server.Features.Comments.UpdateComment;

public class UpdateCommentHandler : IRequestHandler<UpdateCommentCommand, UpdateCommentResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IHubContext<CommentHub> _hubContext;

    public UpdateCommentHandler(ICommentRepository commentRepository, IHubContext<CommentHub> hubContext)
    {
        _commentRepository = commentRepository;
        _hubContext = hubContext;
    }

    public async Task<UpdateCommentResponse> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(request.CommentId);
        if (comment == null || comment.EventId != request.EventId)
        {
            throw new NotFoundException("Comment not found");
        }

        if (comment.UserId != request.UserId)
        {
            throw new ForbiddenException("Not authorized to update this comment");
        }

        comment.Content = request.Content;
        await _commentRepository.UpdateAsync(comment);

        var response = new UpdateCommentResponse(comment.Id, comment.Content, comment.CreatedAt);

        // Broadcast comment update
        await _hubContext.Clients.Group(comment.EventId).SendAsync("CommentUpdated", response, cancellationToken);

        return response;
    }
}
