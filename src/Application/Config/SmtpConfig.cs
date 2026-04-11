namespace Application.Config;

public sealed record SmtpConfig
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string User { get; set; }
    public required string Password { get; set; }
    public required string From { get; set; }
}