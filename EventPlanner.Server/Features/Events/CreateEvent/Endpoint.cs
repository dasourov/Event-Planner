using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Events.CreateEvent;

public class CreateEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/events", async (CreateEventRequest req, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var command = new CreateEventCommand(
                req.Title,
                req.Description,
                req.Location,
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
        .WithName("CreateEvent")
        .WithTags("Events")
        .RequireAuthorization();
    }
}

public record CreateEventRequest(
    string Title,
    string Description,
    string Location,
    double? Latitude,
    double? Longitude,
    System.DateTime Date,
    string CategoryId,
    int? MaxAttendees
);
