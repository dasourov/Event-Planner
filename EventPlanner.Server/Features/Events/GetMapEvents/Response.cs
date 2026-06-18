using System;

namespace EventPlanner.Server.Features.Events.GetMapEvents;

public record GetMapEventsResponse(
    string Id,
    string Title,
    string Location,
    string? ImageUrl,
    double? Latitude,
    double? Longitude,
    DateTime Date
);
