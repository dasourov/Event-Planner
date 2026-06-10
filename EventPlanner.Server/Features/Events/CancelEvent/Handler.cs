using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Events.CancelEvent;

public class CancelEventHandler : IRequestHandler<CancelEventCommand, CancelEventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;

    public CancelEventHandler(IEventRepository eventRepository, IUserRepository userRepository)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
    }

    public async Task<CancelEventResponse> Handle(CancelEventCommand request, CancellationToken cancellationToken)
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
            throw new Exception("Not authorized to cancel this event");
        }

        @event.Status = EventStatus.Cancelled;
        await _eventRepository.UpdateAsync(@event);

        return new CancelEventResponse(@event.Id, @event.Status.ToString());
    }
}
