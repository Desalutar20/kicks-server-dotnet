namespace Application.Config;

public sealed record ApplicationConfig
{
    public required string ClientUrl { get; init; }
    public required string AccountVerificationPath { get; init; }
    public required string ResetPasswordPath { get; init; }

    public required string SessionCookieName { get; init; }
    public required string OAuthStateCookieName { get; init; }
    public required bool CookieSecure { get; init; }

    public required int AccountVerificationTtlMinutes { get; init; }
    public required int SessionTtlMinutes { get; init; }
    public required int OAuthStateTtlMinutes { get; init; }
    public required int ResetPasswordTtlMinutes { get; init; }
}
