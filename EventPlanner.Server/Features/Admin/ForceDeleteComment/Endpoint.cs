using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Admin.ForceDeleteComment;

public class ForceDeleteCommentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/admin/comments/{id}", async (string id, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new ForceDeleteCommentCommand(id, userId));
            return Results.Ok(response);
        })
        .WithName("AdminForceDeleteComment")
        .WithTags("Admin")
        .RequireAuthorization("AdminOnly");
    }
}
