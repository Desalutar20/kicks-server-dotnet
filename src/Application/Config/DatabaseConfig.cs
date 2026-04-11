namespace Application.Config;

public sealed record DatabaseConfig
{
    public required string Host { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
    public required string User { get; set; }
    public required int Port { get; set; }
    public required bool Ssl { get; set; }

    public string GetConnectionString() =>
        $"Host={Host};Port={Port};Database={Name};Username={User};Password={Password}";
}