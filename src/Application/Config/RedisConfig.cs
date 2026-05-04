namespace Application.Config;

public sealed record RedisConfig
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string User { get; init; }
    public required string Password { get; init; }
    public required int Database { get; init; }
    public string? KeyPrefix { get; init; }
}
