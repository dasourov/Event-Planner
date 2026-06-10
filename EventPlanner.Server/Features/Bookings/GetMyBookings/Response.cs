using System;

namespace EventPlanner.Server.Features.Bookings.GetMyBookings;

public record GetMyBookingsResponse(
    string BookingId,
    string EventId,
    string Title,
    string Description,
    string Location,
    DateTime Date,
    string OrganizerName
);
