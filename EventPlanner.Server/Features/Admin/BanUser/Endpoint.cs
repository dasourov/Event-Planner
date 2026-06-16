using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Admin.BanUser;

public class BanUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/admin/users/{userId}/ban", async (string userId, ClaimsPrincipal user, ISender sender) =>
        {
            var adminUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new BanUserCommand(userId, adminUserId));
            return Results.Ok(response);
        })
        .WithName("AdminBanUser")
        .WithTags("Admin")
        .RequireAuthorization("AdminOnly");
    }
}
