using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Admin.GetUsers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, List<GetUsersResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<GetUsersResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var admin = await _userRepository.GetByIdAsync(request.UserId);
        if (admin == null || admin.Role != UserRole.Admin)
        {
            throw new Exception("Unauthorized. Only admins can view user list.");
        }

        var users = await _userRepository.ListAsync();
        return users.Select(u => new GetUsersResponse(
            u.Id,
            u.Username,
            u.Email,
            u.Role.ToString(),
            u.IsBanned,
            u.CreatedAt
        )).ToList();
    }
}
