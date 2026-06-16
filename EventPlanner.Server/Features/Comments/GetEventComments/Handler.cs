using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Comments.GetEventComments;

public class GetEventCommentsHandler : IRequestHandler<GetEventCommentsQuery, List<GetEventCommentsResponse>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUserRepository _userRepository;

    public GetEventCommentsHandler(ICommentRepository commentRepository, IUserRepository userRepository)
    {
        _commentRepository = commentRepository;
        _userRepository = userRepository;
    }

    public async Task<List<GetEventCommentsResponse>> Handle(GetEventCommentsQuery request, CancellationToken cancellationToken)
    {
        var comments = await _commentRepository.ListByEventAsync(request.EventId);

        var responseList = new List<GetEventCommentsResponse>();
        if (comments == null || comments.Count == 0)
        {
            return responseList;
        }

        var userIds = comments.Select(c => c.UserId).Distinct().ToList();
        var users = await _userRepository.GetByIdsAsync(userIds);
        var usersById = users.ToDictionary(u => u.Id);

        // Group replies by their parent so the tree can be built top-down.
        // A comment whose parent is missing (e.g. force-deleted) is treated as top-level.
        var commentIds = comments.Select(c => c.Id).ToHashSet();
        var childrenByParent = comments
            .Where(c => c.ParentCommentId != null && commentIds.Contains(c.ParentCommentId))
            .GroupBy(c => c.ParentCommentId!)
            .ToDictionary(g => g.Key, g => g.ToList());

        GetEventCommentsResponse ToResponse(Comment comment)
        {
            usersById.TryGetValue(comment.UserId, out var user);

            var replies = childrenByParent.TryGetValue(comment.Id, out var children)
                ? children.OrderBy(c => c.CreatedAt).Select(ToResponse).ToList()
                : new List<GetEventCommentsResponse>();

            return new GetEventCommentsResponse(
                comment.Id,
                comment.EventId,
                comment.UserId,
                user?.Username ?? "Unknown",
                comment.Content,
                comment.ParentCommentId,
                comment.CreatedAt,
                replies
            );
        }

        return comments
            .Where(c => c.ParentCommentId == null || !commentIds.Contains(c.ParentCommentId))
            .OrderByDescending(c => c.CreatedAt)
            .Select(ToResponse)
            .ToList();
    }
}
