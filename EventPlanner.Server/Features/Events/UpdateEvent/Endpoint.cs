using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Events.UpdateEvent;

public class UpdateEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/events/{id}", async (string id, UpdateEventRequest req, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var command = new UpdateEventCommand(
                id,
                req.Title,
                req.Description,
                req.Location,
                req.ImageUrl,
                req.Latitude,
                req.Longitude,
                req.Date,
                req.CategoryId,
                req.MaxAttendees,
                userId
            );

            var response = await sender.Send(command);
            return Results.Ok(response);
        })
        .WithName("UpdateEvent")
        .WithTags("Events")
        .RequireAuthorization();
    }
}

public record UpdateEventRequest(
    string Title,
    string Description,
    string Location,
    string? ImageUrl,
    double? Latitude,
    double? Longitude,
    System.DateTime Date,
    string CategoryId,
    int? MaxAttendees
);
