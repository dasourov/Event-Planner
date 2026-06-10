using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Admin.ForceDeleteEvent;

public class ForceDeleteEventHandler : IRequestHandler<ForceDeleteEventCommand, ForceDeleteEventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;

    public ForceDeleteEventHandler(IEventRepository eventRepository, IUserRepository userRepository)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
    }

    public async Task<ForceDeleteEventResponse> Handle(ForceDeleteEventCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null || user.Role != UserRole.Admin)
        {
            throw new Exception("Unauthorized. Only admins can force delete events.");
        }

        var @event = await _eventRepository.GetByIdAsync(request.Id);
        if (@event == null)
        {
            throw new Exception("Event not found");
        }

        await _eventRepository.DeleteAsync(request.Id);
        return new ForceDeleteEventResponse(true);
    }
}
