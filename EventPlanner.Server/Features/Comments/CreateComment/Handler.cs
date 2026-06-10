using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
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
            throw new Exception("Event not found");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        var comment = new Comment
        {
            EventId = request.EventId,
            UserId = request.UserId,
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
            comment.CreatedAt
        );

        // Broadcast to clients in the event room
        await _hubContext.Clients.Group(request.EventId).SendAsync("CommentCreated", response, cancellationToken);

        return response;
    }
}
