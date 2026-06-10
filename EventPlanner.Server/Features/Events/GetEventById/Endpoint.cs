using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Events.GetEventById;

public class GetEventByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/events/{id}", async (string id, ISender sender) =>
        {
            var response = await sender.Send(new GetEventByIdQuery(id));
            return Results.Ok(response);
        })
        .WithName("GetEventById")
        .WithTags("Events")
        .AllowAnonymous();
    }
}
