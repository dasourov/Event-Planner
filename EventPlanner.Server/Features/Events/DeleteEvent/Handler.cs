using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Events.DeleteEvent;

public class DeleteEventHandler : IRequestHandler<DeleteEventCommand, DeleteEventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;

    public DeleteEventHandler(IEventRepository eventRepository, IUserRepository userRepository)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
    }

    public async Task<DeleteEventResponse> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.Id);
        if (@event == null)
        {
            throw new Exception("Event not found");
        }

        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new Exception("User not found");
        }

        if (@event.OrganizerId != request.UserId && user.Role != UserRole.Admin)
        {
            throw new Exception("Not authorized to delete this event");
        }

        await _eventRepository.DeleteAsync(request.Id);
        return new DeleteEventResponse(true);
    }
}
