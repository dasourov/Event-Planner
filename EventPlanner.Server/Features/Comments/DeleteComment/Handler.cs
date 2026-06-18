using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using EventPlanner.Server.Common.Errors;
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
        if (comment == null || comment.EventId != request.EventId)
        {
            throw new NotFoundException("Comment not found");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        // Only author or admin can delete
        if (comment.UserId != request.UserId && user.Role != UserRole.Admin)
        {
            throw new ForbiddenException("Not authorized to delete this comment");
        }

        // Deleting a comment also deletes its replies (and their replies),
        // so no orphaned reply chains are left behind
        var eventComments = await _commentRepository.ListByEventAsync(comment.EventId);
        var idsToDelete = CollectWithDescendants(comment.Id, eventComments);

        await _commentRepository.DeleteManyAsync(idsToDelete);

        // Broadcast each deleted comment so clients can drop them from their state
        foreach (var deletedId in idsToDelete)
        {
            await _hubContext.Clients.Group(comment.EventId).SendAsync("CommentDeleted", deletedId, cancellationToken);
        }

        return new DeleteCommentResponse(true);
    }

    private static List<string> CollectWithDescendants(string rootId, List<Domain.Entities.Comment> comments)
    {
        var childrenByParent = comments
            .Where(c => c.ParentCommentId != null)
            .GroupBy(c => c.ParentCommentId!)
            .ToDictionary(g => g.Key, g => g.Select(c => c.Id).ToList());

        var result = new List<string>();
        var queue = new Queue<string>();
        queue.Enqueue(rootId);

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            result.Add(id);
            if (childrenByParent.TryGetValue(id, out var children))
            {
                foreach (var childId in children)
                {
                    queue.Enqueue(childId);
                }
            }
        }

        return result;
    }
}
