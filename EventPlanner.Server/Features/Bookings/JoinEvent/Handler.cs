using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
            throw new Exception("Event not found");
        }

        if (@event.Status != EventStatus.Published)
        {
            throw new Exception("You can only join published events");
        }

        var existingBooking = await _bookingRepository.GetByUserAndEventAsync(request.UserId, request.EventId);
        if (existingBooking != null)
        {
            throw new Exception("You are already booked for this event");
        }

        if (@event.MaxAttendees.HasValue)
        {
            var currentAttendeesCount = await _bookingRepository.CountByEventAsync(request.EventId);
            if (currentAttendeesCount >= @event.MaxAttendees.Value)
            {
                throw new Exception("Event capacity reached");
            }
        }

        var booking = new Booking
        {
            UserId = request.UserId,
            EventId = request.EventId,
            BookedAt = DateTime.UtcNow
        };

        await _bookingRepository.CreateAsync(booking);
        return new JoinEventResponse(booking.Id, true);
    }
}
