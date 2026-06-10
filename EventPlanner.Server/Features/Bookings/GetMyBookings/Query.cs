using System.Collections.Generic;
using MediatR;

namespace EventPlanner.Server.Features.Bookings.GetMyBookings;

public record GetMyBookingsQuery(string UserId) : IRequest<List<GetMyBookingsResponse>>;
