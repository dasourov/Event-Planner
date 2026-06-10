using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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

        foreach (var comment in comments)
        {
            var user = await _userRepository.GetByIdAsync(comment.UserId);
            responseList.Add(new GetEventCommentsResponse(
                comment.Id,
                comment.EventId,
                comment.UserId,
                user?.Username ?? "Unknown",
                comment.Content,
                comment.CreatedAt
            ));
        }

        return responseList.OrderByDescending(c => c.CreatedAt).ToList();
    }
}
