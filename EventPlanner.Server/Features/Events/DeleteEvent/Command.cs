using MediatR;

namespace EventPlanner.Server.Features.Events.DeleteEvent;

public record DeleteEventCommand(string Id, string UserId) : IRequest<DeleteEventResponse>;
