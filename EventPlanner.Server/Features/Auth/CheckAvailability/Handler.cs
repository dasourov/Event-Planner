using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Auth.CheckAvailability;

public class CheckAvailabilityHandler : IRequestHandler<CheckAvailabilityQuery, CheckAvailabilityResponse>
{
    private readonly IUserRepository _userRepository;

    public CheckAvailabilityHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<CheckAvailabilityResponse> Handle(CheckAvailabilityQuery request, CancellationToken cancellationToken)
    {
        bool usernameExists = false;
        bool emailExists = false;

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username.Trim());
            usernameExists = user != null;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var user = await _userRepository.GetByEmailAsync(request.Email.Trim());
            emailExists = user != null;
        }

        return new CheckAvailabilityResponse(usernameExists, emailExists);
    }
}
