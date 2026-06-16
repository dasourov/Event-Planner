using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Admin.ForceDeleteEvent;

public class ForceDeleteEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/admin/events/{id}", async (string id, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new ForceDeleteEventCommand(id, userId));
            return Results.Ok(response);
        })
        .WithName("AdminForceDeleteEvent")
        .WithTags("Admin")
        .RequireAuthorization("AdminOnly");
    }
}
