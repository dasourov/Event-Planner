using System;

namespace EventPlanner.Server.Features.Events.UpdateEvent;

public record UpdateEventResponse(
    string Id,
    string Title,
    string Description,
    string Location,
    double? Latitude,
    double? Longitude,
    DateTime Date,
    string CategoryId,
    int? MaxAttendees,
    string Status
);
