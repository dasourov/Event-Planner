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
        app.MapGet("/api/v1/events", async (
            string? categoryId,
            string? searchTerm,
            string? status,
            int? page,
            int? pageSize,
            ISender sender) =>
        {
            var response = await sender.Send(new GetEventsQuery(
                categoryId,
                searchTerm,
                status,
                page ?? 1,
                pageSize ?? 20
            ));

            return Results.Ok(response);
        })
        .WithName("GetEvents")
        .WithTags("Events")
        .AllowAnonymous();
    }
}