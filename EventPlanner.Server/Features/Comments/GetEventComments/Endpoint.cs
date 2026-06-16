using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Comments.GetEventComments;

public class GetEventCommentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/events/{eventId}/comments", async (string eventId, ISender sender) =>
        {
            var response = await sender.Send(new GetEventCommentsQuery(eventId));
            return Results.Ok(response);
        })
        .WithName("GetEventComments")
        .WithTags("Comments")
        .AllowAnonymous();
    }
}
