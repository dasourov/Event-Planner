using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Events.GetEventById;

public class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, GetEventByIdResponse>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBookingRepository _bookingRepository;

    public GetEventByIdHandler(
        IEventRepository eventRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IBookingRepository bookingRepository
    )
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<GetEventByIdResponse> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.Id);
        if (@event == null)
        {
            throw new Exception("Event not found");
        }

        var organizer = await _userRepository.GetByIdAsync(@event.OrganizerId);
        var category = await _categoryRepository.GetByIdAsync(@event.CategoryId);
        var attendeeCount = await _bookingRepository.CountByEventAsync(@event.Id);

        return new GetEventByIdResponse(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.Location,
            @event.ImageUrl,
            @event.Latitude,
            @event.Longitude,
            @event.Date,
            @event.CategoryId,
            category?.Name ?? "Unknown",
            @event.MaxAttendees,
            attendeeCount,
            @event.Status.ToString(),
            @event.OrganizerId,
            organizer?.Username ?? "Unknown"
        );
    }
}
