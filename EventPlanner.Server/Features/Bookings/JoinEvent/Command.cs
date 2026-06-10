using MediatR;

namespace EventPlanner.Server.Features.Bookings.JoinEvent;

public record JoinEventCommand(string EventId, string UserId) : IRequest<JoinEventResponse>;
