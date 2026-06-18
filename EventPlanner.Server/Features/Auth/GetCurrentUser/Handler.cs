using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Common.Errors;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Auth.GetCurrentUser;

public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, GetCurrentUserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (user.IsBanned)
        {
            throw new ForbiddenException("Your account has been banned.");
        }

        return new GetCurrentUserResponse(user.Id, user.Username, user.Email, user.Role.ToString());
    }
}
