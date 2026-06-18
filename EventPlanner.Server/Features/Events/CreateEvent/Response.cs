using System;

namespace EventPlanner.Server.Features.Events.CreateEvent;

public record CreateEventResponse(
    string Id,
    string Title,
    string Description,
    string Location,
    string? ImageUrl,
    double? Latitude,
    double? Longitude,
    DateTime Date,
    string CategoryId,
    int? MaxAttendees,
    string Status,
    string OrganizerId
);
