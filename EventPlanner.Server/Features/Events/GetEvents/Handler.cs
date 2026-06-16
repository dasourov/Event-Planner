using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Events.GetEvents;

public class GetEventsHandler : IRequestHandler<GetEventsQuery, List<GetEventsResponse>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBookingRepository _bookingRepository;

    public GetEventsHandler(
        IEventRepository eventRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IBookingRepository bookingRepository
    )
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<List<GetEventsResponse>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var events = await _eventRepository.ListAsync(
            request.CategoryId,
            request.SearchTerm,
            request.Status,
            page,
            pageSize
        );

        var resultList = new List<GetEventsResponse>();

        if (events.Count == 0) return resultList;

        var organizerIds = new HashSet<string>();
        var categoryIds = new HashSet<string>();
        var eventIds = new HashSet<string>();

        foreach (var @event in events)
        {
            organizerIds.Add(@event.OrganizerId);
            categoryIds.Add(@event.CategoryId);
            eventIds.Add(@event.Id);
        }

        var organizersTask = _userRepository.GetByIdsAsync(organizerIds);
        var categoriesTask = _categoryRepository.GetByIdsAsync(categoryIds);
        var attendeeCountsTask = _bookingRepository.GetAttendeeCountsAsync(eventIds);

        await Task.WhenAll(organizersTask, categoriesTask, attendeeCountsTask);

        var organizersDict = new Dictionary<string, Domain.Entities.User>();
        foreach (var org in organizersTask.Result) organizersDict[org.Id] = org;

        var categoriesDict = new Dictionary<string, Domain.Entities.Category>();
        foreach (var cat in categoriesTask.Result) categoriesDict[cat.Id] = cat;

        var attendeeCountsDict = attendeeCountsTask.Result;

        foreach (var @event in events)
        {
            organizersDict.TryGetValue(@event.OrganizerId, out var organizer);
            categoriesDict.TryGetValue(@event.CategoryId, out var category);
            attendeeCountsDict.TryGetValue(@event.Id, out var attendeeCount);

            resultList.Add(new GetEventsResponse(
                @event.Id,
                @event.Title,
                @event.Description,
                @event.Location,
                @event.Latitude,
                @event.Longitude,
                @event.Date,
                @event.CategoryId,
                category?.Name ?? "Unknown",
                @event.MaxAttendees,
                attendeeCount,
                @event.Status.ToString(),
                @event.OrganizerId,
                organizer?.Username ?? "Unknown"
            ));
        }

        return resultList;
    }
}