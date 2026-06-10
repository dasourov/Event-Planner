using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Bookings.GetEventAttendees;

public class GetEventAttendeesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events/{eventId}/attendees", async (string eventId, ISender sender) =>
        {
            var response = await sender.Send(new GetEventAttendeesQuery(eventId));
            return Results.Ok(response);
        })
        .WithName("GetEventAttendees")
        .WithTags("Bookings")
        .RequireAuthorization();
    }
}
