using System.Collections.Generic;
using MediatR;

namespace EventPlanner.Server.Features.Events.GetEvents;

public record GetEventsQuery(string? CategoryId = null, string? SearchTerm = null) : IRequest<List<GetEventsResponse>>;
