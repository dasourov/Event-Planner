using System.Collections.Generic;
using MediatR;

namespace EventPlanner.Server.Features.Events.GetEvents;

public record GetEventsQuery(
    string? CategoryId = null,
    string? SearchTerm = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<List<GetEventsResponse>>;