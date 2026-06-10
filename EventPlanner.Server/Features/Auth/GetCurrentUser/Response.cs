namespace EventPlanner.Server.Features.Auth.GetCurrentUser;

public record GetCurrentUserResponse(string Id, string Username, string Email, string Role);
