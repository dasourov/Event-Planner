using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Comments.UpdateComment;

public class UpdateCommentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/events/{eventId}/comments/{commentId}", async (string eventId, string commentId, UpdateCommentRequest req, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new UpdateCommentCommand(eventId, commentId, userId, req.Content));
            return Results.Ok(response);
        })
        .WithName("UpdateComment")
        .WithTags("Comments")
        .RequireAuthorization();
    }
}

public record UpdateCommentRequest(string Content);
