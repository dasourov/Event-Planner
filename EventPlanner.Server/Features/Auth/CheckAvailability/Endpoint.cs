using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Auth.CheckAvailability;

public class CheckAvailabilityEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/auth/check-availability", async (
            [FromQuery] string? username,
            [FromQuery] string? email,
            ISender sender) =>
        {
            var response = await sender.Send(new CheckAvailabilityQuery(username, email));
            return Results.Ok(response);
        })
        .WithName("CheckAvailability")
        .WithTags("Auth")
        .AllowAnonymous();
    }
}
