using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;
using EventPlanner.Server.Common.Errors;

namespace EventPlanner.Server.Features.Admin.BanUser;

public class BanUserHandler : IRequestHandler<BanUserCommand, BanUserResponse>
{
    private readonly IUserRepository _userRepository;

    public BanUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<BanUserResponse> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var admin = await _userRepository.GetByIdAsync(request.AdminUserId);
        if (admin == null || admin.Role != UserRole.Admin)
        {
            throw new ForbiddenException("Only admins can ban users.");
        }

        var targetUser = await _userRepository.GetByIdAsync(request.TargetUserId);
        if (targetUser == null)
        {
            throw new NotFoundException("Target user not found");
        }

        targetUser.IsBanned = true;
        await _userRepository.UpdateAsync(targetUser);

        return new BanUserResponse(true);
    }
}
