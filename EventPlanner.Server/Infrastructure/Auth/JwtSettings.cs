namespace EventPlanner.Server.Infrastructure.Auth;

public class JwtSettings
{
    public string Secret { get; set; } = "super_secret_key_that_is_at_least_32_characters_long_12345!";
    public string Issuer { get; set; } = "EventPlanner";
    public string Audience { get; set; } = "EventPlanner";
    public int ExpiryMinutes { get; set; } = 60;
}
