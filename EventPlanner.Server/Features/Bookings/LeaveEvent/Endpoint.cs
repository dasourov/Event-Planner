using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Bookings.LeaveEvent;

public class LeaveEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/bookings/{eventId}/leave", async (string eventId, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new LeaveEventCommand(eventId, userId));
            return Results.Ok(response);
        })
        .WithName("LeaveEvent")
        .WithTags("Bookings")
        .RequireAuthorization();
    }
}
