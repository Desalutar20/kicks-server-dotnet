namespace Application.Config;

public sealed record SmtpConfig
{
    public required string Host { get; init; }
    public required int Port { get; init; }
    public required string User { get; init; }
    public required string Password { get; init; }
    public required string From { get; init; }
}
