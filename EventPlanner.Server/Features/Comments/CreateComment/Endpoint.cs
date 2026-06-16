using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Comments.CreateComment;

public class CreateCommentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/events/{eventId}/comments", async (string eventId, CreateCommentRequest req, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new CreateCommentCommand(eventId, userId, req.Content));
            return Results.Ok(response);
        })
        .WithName("CreateComment")
        .WithTags("Comments")
        .RequireAuthorization();
    }
}

public record CreateCommentRequest(string Content);
