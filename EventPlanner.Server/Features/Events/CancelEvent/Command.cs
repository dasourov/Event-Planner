using MediatR;

namespace EventPlanner.Server.Features.Events.CancelEvent;

public record CancelEventCommand(string Id, string UserId) : IRequest<CancelEventResponse>;
