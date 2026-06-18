using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Events.GetMapEvents;

public class GetMapEventsHandler : IRequestHandler<GetMapEventsQuery, List<GetMapEventsResponse>>
{
    private readonly IEventRepository _eventRepository;

    public GetMapEventsHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<List<GetMapEventsResponse>> Handle(GetMapEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _eventRepository.ListPublishedAsync();
        return events
            .Where(e => e.Latitude.HasValue && e.Longitude.HasValue)
            .Select(e => new GetMapEventsResponse(
                e.Id,
                e.Title,
                e.Location,
                e.ImageUrl,
                e.Latitude,
                e.Longitude,
                e.Date
            ))
            .ToList();
    }
}
