using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Categories.GetCategories;

public class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/categories", async (ISender sender) =>
        {
            var response = await sender.Send(new GetCategoriesQuery());
            return Results.Ok(response);
        })
        .WithName("GetCategories")
        .WithTags("Categories")
        .AllowAnonymous();
    }
}
