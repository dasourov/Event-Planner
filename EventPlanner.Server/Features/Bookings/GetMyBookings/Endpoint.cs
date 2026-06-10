using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using EventPlanner.Server.Common.Endpoints;

namespace EventPlanner.Server.Features.Bookings.GetMyBookings;

public class GetMyBookingsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/bookings/my", async (ClaimsPrincipal user, ISender sender) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var response = await sender.Send(new GetMyBookingsQuery(userId));
            return Results.Ok(response);
        })
        .WithName("GetMyBookings")
        .WithTags("Bookings")
        .RequireAuthorization();
    }
}
