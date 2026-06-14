using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Common.Errors;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Bookings.JoinEvent;

public class JoinEventHandler : IRequestHandler<JoinEventCommand, JoinEventResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;

    public JoinEventHandler(IBookingRepository bookingRepository, IEventRepository eventRepository)
    {
        _bookingRepository = bookingRepository;
        _eventRepository = eventRepository;
    }

    public async Task<JoinEventResponse> Handle(JoinEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.EventId);
        if (@event == null)
        {
            throw new NotFoundException("Event not found");
        }

        if (@event.Status != EventStatus.Published)
        {
            throw new ConflictException("You can only join published events");
        }

        var existingBooking = await _bookingRepository.GetByUserAndEventAsync(request.UserId, request.EventId);
        if (existingBooking != null)
        {
            throw new ConflictException("You are already booked for this event");
        }

        if (@event.MaxAttendees.HasValue)
        {
            var currentAttendeesCount = await _bookingRepository.CountByEventAsync(request.EventId);
            if (currentAttendeesCount >= @event.MaxAttendees.Value)
            {
                throw new ConflictException("Event capacity reached");
            }
        }

        var booking = new Booking
        {
            UserId = request.UserId,
            EventId = request.EventId,
            BookedAt = DateTime.UtcNow
        };

        await _bookingRepository.CreateAsync(booking);

        // The pre-insert capacity check above can race: two simultaneous joins can both
        // pass it and oversell the event. After inserting, rank all bookings by Id
        // (ObjectIds sort by creation order) and roll back if this booking landed
        // outside capacity. Ranking is deterministic, so concurrent racers agree on
        // which booking is the excess one.
        if (@event.MaxAttendees.HasValue)
        {
            var allBookings = await _bookingRepository.ListByEventAsync(request.EventId);
            var withinCapacity = allBookings
                .OrderBy(b => b.Id, StringComparer.Ordinal)
                .Take(@event.MaxAttendees.Value)
                .Any(b => b.Id == booking.Id);

            if (!withinCapacity)
            {
                await _bookingRepository.DeleteAsync(booking.Id);
                throw new ConflictException("Event capacity reached");
            }
        }

        return new JoinEventResponse(booking.Id, true);
    }
}
