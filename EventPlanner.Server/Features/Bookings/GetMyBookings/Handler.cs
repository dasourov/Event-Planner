using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Bookings.GetMyBookings;

public class GetMyBookingsHandler : IRequestHandler<GetMyBookingsQuery, List<GetMyBookingsResponse>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;

    public GetMyBookingsHandler(
        IBookingRepository bookingRepository,
        IEventRepository eventRepository,
        IUserRepository userRepository
    )
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
        _userRepository = userRepository;
    }

    public async Task<List<GetMyBookingsResponse>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.ListByUserAsync(request.UserId);

        var resultList = new List<GetMyBookingsResponse>();
        if (bookings == null || bookings.Count == 0)
        {
            return resultList;
        }

        var eventIds = bookings.Select(b => b.EventId).Distinct().ToList();
        var events = await _eventRepository.GetByIdsAsync(eventIds);
        var eventsDict = events.ToDictionary(e => e.Id);

        var organizerIds = events.Select(e => e.OrganizerId).Distinct().ToList();
        var organizers = await _userRepository.GetByIdsAsync(organizerIds);
        var organizersDict = organizers.ToDictionary(u => u.Id);

        foreach (var booking in bookings)
        {
            if (eventsDict.TryGetValue(booking.EventId, out var @event))
            {
                organizersDict.TryGetValue(@event.OrganizerId, out var organizer);
                resultList.Add(new GetMyBookingsResponse(
                    booking.Id,
                    @event.Id,
                    @event.Title,
                    @event.Description,
                    @event.Location,
                    @event.Date,
                    organizer?.Username ?? "Unknown"
                ));
            }
        }

        return resultList;
    }
}
