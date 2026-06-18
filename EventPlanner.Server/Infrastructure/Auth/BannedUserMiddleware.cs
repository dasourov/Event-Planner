using System.Security.Claims;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Infrastructure.Auth;

/// <summary>
/// Rejects authenticated requests when the user's account has been banned,
/// so a ban takes effect immediately without waiting for JWT expiry.
/// </summary>
public class BannedUserMiddleware
{
    private readonly RequestDelegate _next;

    public BannedUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await userRepository.GetByIdAsync(userId);
                if (user?.IsBanned == true)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/problem+json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        status = 403,
                        title = "Forbidden",
                        detail = "Your account has been banned."
                    });
                    return;
                }
            }
        }

        await _next(context);
    }
}
