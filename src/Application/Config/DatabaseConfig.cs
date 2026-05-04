namespace Application.Config;

public sealed record DatabaseConfig
{
    public required string Host { get; init; }
    public required string Password { get; init; }
    public required string Name { get; init; }
    public required string User { get; init; }
    public required int Port { get; init; }
    public required bool Ssl { get; init; }

    public string GetConnectionString() =>
        $"Host={Host};Port={Port};Database={Name};Username={User};Password={Password}";
}
