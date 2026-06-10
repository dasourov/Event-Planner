using System.Collections.Generic;
using MediatR;

namespace EventPlanner.Server.Features.Bookings.GetEventAttendees;

public record GetEventAttendeesQuery(string EventId) : IRequest<List<GetEventAttendeesResponse>>;
