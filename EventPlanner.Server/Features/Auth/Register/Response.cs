namespace EventPlanner.Server.Features.Auth.Register;

public record RegisterResponse(string Id, string Username, string Email, string Token);
