using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Events.GetEvents;

public class GetEventsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events", async (string? categoryId, string? searchTerm, ISender sender) =>
        {
            var response = await sender.Send(new GetEventsQuery(categoryId, searchTerm));
            return Results.Ok(response);
        })
        .WithName("GetEvents")
        .WithTags("Events")
        .AllowAnonymous();
    }
}
