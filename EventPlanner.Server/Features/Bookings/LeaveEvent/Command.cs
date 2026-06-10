using MediatR;

namespace EventPlanner.Server.Features.Bookings.LeaveEvent;

public record LeaveEventCommand(string EventId, string UserId) : IRequest<LeaveEventResponse>;
