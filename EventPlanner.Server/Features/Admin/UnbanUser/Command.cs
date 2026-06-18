using MediatR;

namespace EventPlanner.Server.Features.Admin.UnbanUser;

public record UnbanUserCommand(string TargetUserId, string AdminUserId) : IRequest<UnbanUserResponse>;
