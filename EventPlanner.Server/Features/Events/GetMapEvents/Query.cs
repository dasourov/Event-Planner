using System.Collections.Generic;
using MediatR;

namespace EventPlanner.Server.Features.Events.GetMapEvents;

public record GetMapEventsQuery() : IRequest<List<GetMapEventsResponse>>;
