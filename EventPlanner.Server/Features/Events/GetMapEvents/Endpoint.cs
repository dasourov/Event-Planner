using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Events.GetMapEvents;

public class GetMapEventsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/events/map", async (ISender sender) =>
        {
            var response = await sender.Send(new GetMapEventsQuery());
            return Results.Ok(response);
        })
        .WithName("GetMapEvents")
        .WithTags("Events")
        .AllowAnonymous();
    }
}
