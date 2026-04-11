namespace Application.Config;

public sealed record Config
{
    public required ApplicationConfig Application { get; set; }
    public required DatabaseConfig Database { get; set; }
    public required RedisConfig Redis { get; set; }
    public required SmtpConfig Smtp { get; set; }
    public required RateLimitConfig RateLimit { get; set; }
}