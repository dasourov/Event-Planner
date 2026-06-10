using Microsoft.AspNetCore.Identity;

namespace EventPlanner.Server.Infrastructure.Auth;

public class PasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();
    private static readonly object DummyUser = new();

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(DummyUser, password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(DummyUser, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}
