using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Auth.Login;

public class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", async (LoginCommand command, ISender sender) =>
        {
            var response = await sender.Send(command);
            return Results.Ok(response);
        })
        .WithName("Login")
        .WithTags("Auth")
        .AllowAnonymous();
    }
}
