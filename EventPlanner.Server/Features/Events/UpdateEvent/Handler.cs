using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Events.UpdateEvent;

public class UpdateEventHandler : IRequestHandler<UpdateEventCommand, UpdateEventResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUserRepository _userRepository;

    public UpdateEventHandler(
        IEventRepository eventRepository,
        ICategoryRepository categoryRepository,
        IUserRepository userRepository
    )
    {
        _eventRepository = eventRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    public async Task<UpdateEventResponse> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
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

        // Only organizer or Admin can update
        if (@event.OrganizerId != request.UserId && user.Role != UserRole.Admin)
        {
            throw new Exception("Not authorized to update this event");
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            throw new Exception("Category not found");
        }

        @event.Title = request.Title;
        @event.Description = request.Description;
        @event.Location = request.Location;
        @event.Latitude = request.Latitude;
        @event.Longitude = request.Longitude;
        @event.Date = request.Date;
        @event.CategoryId = request.CategoryId;
        @event.MaxAttendees = request.MaxAttendees;

        await _eventRepository.UpdateAsync(@event);

        return new UpdateEventResponse(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.Location,
            @event.Latitude,
            @event.Longitude,
            @event.Date,
            @event.CategoryId,
            @event.MaxAttendees,
            @event.Status.ToString()
        );
    }
}
