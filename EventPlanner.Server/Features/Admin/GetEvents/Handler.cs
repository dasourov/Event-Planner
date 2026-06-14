using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Admin.GetEvents;

public class GetEventsHandler : IRequestHandler<GetEventsQuery, List<GetEventsResponse>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBookingRepository _bookingRepository;

    public GetEventsHandler(
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

    public async Task<List<GetEventsResponse>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _eventRepository.ListAsync();
        var resultList = new List<GetEventsResponse>();

        foreach (var @event in events)
        {
            var organizer = await _userRepository.GetByIdAsync(@event.OrganizerId);
            var category = await _categoryRepository.GetByIdAsync(@event.CategoryId);
            var attendeeCount = await _bookingRepository.CountByEventAsync(@event.Id);

            resultList.Add(new GetEventsResponse(
                @event.Id,
                @event.Title,
                @event.Description,
                @event.Location,
                @event.Latitude,
                @event.Longitude,
                @event.Date,
                @event.CategoryId,
                category?.Name ?? "Unknown",
                @event.MaxAttendees,
                attendeeCount,
                @event.Status.ToString(),
                @event.OrganizerId,
                organizer?.Username ?? "Unknown",
                @event.CreatedAt
            ));
        }

        return resultList;
    }
}