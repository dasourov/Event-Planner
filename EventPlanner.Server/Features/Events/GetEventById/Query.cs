using MediatR;

namespace EventPlanner.Server.Features.Events.GetEventById;

public record GetEventByIdQuery(string Id) : IRequest<GetEventByIdResponse>;
