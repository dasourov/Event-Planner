using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Common.Errors;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Admin.UnbanUser;

public class UnbanUserHandler : IRequestHandler<UnbanUserCommand, UnbanUserResponse>
{
    private readonly IUserRepository _userRepository;

    public UnbanUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UnbanUserResponse> Handle(UnbanUserCommand request, CancellationToken cancellationToken)
    {
        var admin = await _userRepository.GetByIdAsync(request.AdminUserId);
        if (admin == null || admin.Role != UserRole.Admin)
        {
            throw new ForbiddenException("Unauthorized. Only admins can unban users.");
        }

        var targetUser = await _userRepository.GetByIdAsync(request.TargetUserId);
        if (targetUser == null)
        {
            throw new NotFoundException("Target user not found");
        }

        targetUser.IsBanned = false;
        await _userRepository.UpdateAsync(targetUser);

        return new UnbanUserResponse(true);
    }
}
