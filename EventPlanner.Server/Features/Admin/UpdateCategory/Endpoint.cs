using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Admin.UpdateCategory;

public class UpdateCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/admin/categories/{id}", async (string id, UpdateCategoryRequest req, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new UpdateCategoryCommand(id, req.Name, req.Description, userId));
            return Results.Ok(response);
        })
        .WithName("AdminUpdateCategory")
        .WithTags("Admin")
        .RequireAuthorization("AdminOnly");
    }
}

public record UpdateCategoryRequest(string Name, string Description);
