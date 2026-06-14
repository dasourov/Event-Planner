using System.Collections.Generic;
using MediatR;

namespace EventPlanner.Server.Features.Admin.GetEvents;

public record GetEventsQuery(string EventId) : IRequest<List<GetEventsResponse>>;
