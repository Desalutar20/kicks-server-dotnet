namespace Application.Abstractions.OAuth;

public interface IOAuthClient
{
    Uri GenerateRedirectUrl(OAuthState state);
    bool IsValidState(OAuthState received, OAuthState expected);
    Task<Result<OAuthUser>> GetUserAsync(NonEmptyString code, CancellationToken ct = default);
}