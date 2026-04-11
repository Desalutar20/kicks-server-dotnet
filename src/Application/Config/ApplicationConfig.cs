namespace Application.Config;

public sealed record ApplicationConfig
{
    public required string ClientUrl { get; set; }
    public required string AccountVerificationPath { get; set; }
    public required string ResetPasswordPath { get; set; }

    public required string SessionCookieName { get; set; }
    public required bool CookieSecure { get; set; }

    public required int AccountVerificationTtlMinutes { get; set; }
    public required int SessionTtlMinutes { get; set; }
    public required int ResetPasswordTtlMinutes { get; set; }
}