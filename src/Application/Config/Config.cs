namespace Application.Config;

public sealed record Config
{
    public required ApplicationConfig Application { get; init; }
    public required DatabaseConfig Database { get; init; }
    public required RedisConfig Redis { get; init; }
    public required SmtpConfig Smtp { get; init; }
    public required OAuthConfig OAuth { get; init; }
    public required CloudinaryConfig Cloudinary { get; init; }
    public required RateLimitConfig RateLimit { get; init; }
}
