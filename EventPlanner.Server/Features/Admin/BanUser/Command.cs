using MediatR;

namespace EventPlanner.Server.Features.Admin.BanUser;

public record BanUserCommand(string TargetUserId, string AdminUserId) : IRequest<BanUserResponse>;
