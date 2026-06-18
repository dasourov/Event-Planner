using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Auth;
using EventPlanner.Server.Infrastructure.Repositories;
using EventPlanner.Server.Common.Errors;

namespace EventPlanner.Server.Features.Auth.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public RegisterHandler(IUserRepository userRepository, PasswordHasher passwordHasher, JwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            throw new ConflictException("Email already registered");
        }

        var existingUsername = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingUsername != null)
        {
            throw new ConflictException("Username already taken");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = request.IsOrganizer ? UserRole.Organizer : UserRole.User,
            IsOrganizer = request.IsOrganizer,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        var token = _jwtTokenService.GenerateToken(user);
        return new RegisterResponse(user.Id, user.Username, user.Email, token);
    }
}
