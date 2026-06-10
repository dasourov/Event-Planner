using MediatR;

namespace EventPlanner.Server.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;
