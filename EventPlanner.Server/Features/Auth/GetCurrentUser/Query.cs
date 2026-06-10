using MediatR;

namespace EventPlanner.Server.Features.Auth.GetCurrentUser;

public record GetCurrentUserQuery(string UserId) : IRequest<GetCurrentUserResponse>;
