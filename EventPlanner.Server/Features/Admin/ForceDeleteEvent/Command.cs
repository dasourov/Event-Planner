using MediatR;

namespace EventPlanner.Server.Features.Admin.ForceDeleteEvent;

public record ForceDeleteEventCommand(string Id, string UserId) : IRequest<ForceDeleteEventResponse>;
