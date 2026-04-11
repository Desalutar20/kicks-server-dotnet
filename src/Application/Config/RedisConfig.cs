namespace Application.Config;

public sealed record RedisConfig
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string User { get; set; }
    public required string Password { get; set; }
    public required int Database { get; set; }
    public string? KeyPrefix { get; set; }
}