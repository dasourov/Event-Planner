using MediatR;

namespace EventPlanner.Server.Features.Auth.Register;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<RegisterResponse>;
