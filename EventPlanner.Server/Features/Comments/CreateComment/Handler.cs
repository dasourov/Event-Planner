using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using EventPlanner.Server.Common.Errors;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Infrastructure.Repositories;
using EventPlanner.Server.Infrastructure.SignalR;

namespace EventPlanner.Server.Features.Comments.CreateComment;

public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, CreateCommentResponse>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<CommentHub> _hubContext;

    public CreateCommentHandler(
        ICommentRepository commentRepository,
        IEventRepository eventRepository,
        IUserRepository userRepository,
        IHubContext<CommentHub> hubContext
    )
    {
        _commentRepository = commentRepository;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _hubContext = hubContext;
    }

    public async Task<CreateCommentResponse> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null)
        {
            throw new NotFoundException("Event not found");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        // When replying, the parent comment must exist and belong to the same event
        if (!string.IsNullOrEmpty(request.ParentCommentId))
        {
            var parent = await _commentRepository.GetByIdAsync(request.ParentCommentId);
            if (parent == null)
            {
                throw new NotFoundException("Parent comment not found");
            }

            if (parent.EventId != request.EventId)
            {
                throw new ConflictException("Parent comment does not belong to this event");
            }
        }

        var comment = new Comment
        {
            EventId = request.EventId,
            UserId = request.UserId,
            ParentCommentId = string.IsNullOrEmpty(request.ParentCommentId) ? null : request.ParentCommentId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.CreateAsync(comment);

        var response = new CreateCommentResponse(
            comment.Id,
            comment.EventId,
            comment.UserId,
            user.Username,
            comment.Content,
            comment.ParentCommentId,
            comment.CreatedAt
        );

        // Broadcast to clients in the event room
        await _hubContext.Clients.Group(request.EventId).SendAsync("CommentCreated", response, cancellationToken);

        return response;
    }
}
