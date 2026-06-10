namespace EventPlanner.Server.Features.Auth.Login;

public record LoginResponse(string Id, string Username, string Email, string Token);
