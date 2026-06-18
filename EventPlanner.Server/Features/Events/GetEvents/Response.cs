using System;
using System.Collections.Generic;

namespace EventPlanner.Server.Features.Events.GetEvents;

public record GetEventsResponse(
    string Id,
    string Title,
    string Description,
    string Location,
    string? ImageUrl,
    double? Latitude,
    double? Longitude,
    DateTime Date,
    string CategoryId,
    string CategoryName,
    int? MaxAttendees,
    int AttendeeCount,
    string Status,
    string OrganizerId,
    string OrganizerName
);

public record PaginatedEventsResponse(
    List<GetEventsResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
