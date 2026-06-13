using System;

namespace EventPlanner.Server.Features.Admin.GetEvents;

public record GetEventsResponse(
    string Id,
    string Title,
    string Description,
    string Location,
    double? Latitude,
    double? Longitude,
    DateTime Date,
    string CategoryId,
    string CategoryName,
    int? MaxAttendees,
    int AttendeeCount,
    string Status,
    string OrganizerId,
    string OrganizerName,
    DateTime CreatedAt
);
