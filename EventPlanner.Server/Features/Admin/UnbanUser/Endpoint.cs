using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Admin.UnbanUser;

public class UnbanUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/admin/users/{userId}/unban", async (string userId, ClaimsPrincipal user, ISender sender) =>
        {
            var adminUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new UnbanUserCommand(userId, adminUserId));
            return Results.Ok(response);
        })
        .WithName("AdminUnbanUser")
        .WithTags("Admin")
        .RequireAuthorization("AdminOnly");
    }
}
