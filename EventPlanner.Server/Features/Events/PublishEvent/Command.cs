using MediatR;

namespace EventPlanner.Server.Features.Events.PublishEvent;

public record PublishEventCommand(string Id, string UserId) : IRequest<PublishEventResponse>;
