using EventPlanner.Server.Common.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace EventPlanner.Server.Features.Events.UploadImage;

public class UploadImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/events/upload-image", async ([FromForm] IFormFile file, ISender sender) =>
        {
            var command = new UploadImageCommand(file);
            var result = await sender.Send(command);
            return Results.Ok(new { success = true, data = result });
        })
        .RequireAuthorization()
        .DisableAntiforgery()
        .WithTags("Events")
        .WithSummary("Uploads an image file and returns its URL.");
    }
}
