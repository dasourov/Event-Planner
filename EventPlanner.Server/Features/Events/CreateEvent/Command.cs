using System;
using MediatR;

namespace EventPlanner.Server.Features.Events.CreateEvent;

public record CreateEventCommand(
    string Title,
    string Description,
    string Location,
    string? ImageUrl,
    double? Latitude,
    double? Longitude,
    DateTime Date,
    string CategoryId,
    int? MaxAttendees,
    string OrganizerId
) : IRequest<CreateEventResponse>;
