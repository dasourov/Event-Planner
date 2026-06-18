using System;

namespace EventPlanner.Server.Features.Events.GetEventById;

public record GetEventByIdResponse(
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
