using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Infrastructure.Auth;
using EventPlanner.Server.Infrastructure.Repositories;

namespace EventPlanner.Server.Features.Auth.Login;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public LoginHandler(IUserRepository userRepository, PasswordHasher passwordHasher, JwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            throw new Exception("Invalid email or password");
        }

        if (user.IsBanned)
        {
            throw new Exception("Your account is banned.");
        }

        var token = _jwtTokenService.GenerateToken(user);
        return new LoginResponse(user.Id, user.Username, user.Email, token);
    }
}
