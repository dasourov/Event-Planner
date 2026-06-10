using System;
using MediatR;

namespace EventPlanner.Server.Features.Events.UpdateEvent;

public record UpdateEventCommand(
    string Id,
    string Title,
    string Description,
    string Location,
    double? Latitude,
    double? Longitude,
    DateTime Date,
    string CategoryId,
    int? MaxAttendees,
    string UserId
) : IRequest<UpdateEventResponse>;
