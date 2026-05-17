using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.OAuth;

namespace Infrastructure.Services.OAuth;

internal sealed record GoogleAccessTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken
);

internal sealed record GoogleUserResponse(
    [property: JsonPropertyName("sub")] string Sub,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("email_verified")] bool EmailVerified
);

internal sealed class GoogleService(HttpClient httpClient, Config config) : IOAuthClient
{
    private OAuthProviderConfig Cfg => config.OAuth.Google;

    public Uri GenerateRedirectUrl(OAuthState state) =>
        new Uri("https://accounts.google.com/o/oauth2/v2/auth")
            .AddParameter("client_id", Cfg.ClientId)
            .AddParameter("redirect_uri", Cfg.RedirectUrl)
            .AddParameter("response_type", "code")
            .AddParameter("scope", "openid email profile")
            .AddParameter("access_type", "offline")
            .AddParameter("state", state.ToString());

    public bool IsValidState(OAuthState received, OAuthState expected) => received == expected;

    public async Task<Result<OAuthUser>> GetUserAsync(
        NonEmptyString code,
        CancellationToken ct = default
    )
    {
        try
        {
            var tokenRequest = new Dictionary<string, string>
            {
                ["code"] = code.Value,
                ["client_id"] = Cfg.ClientId,
                ["client_secret"] = Cfg.ClientSecret,
                ["redirect_uri"] = Cfg.RedirectUrl,
                ["grant_type"] = "authorization_code",
            };

            var tokenResponse = await httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(tokenRequest),
                ct
            );

            var tokenJson = await tokenResponse.Content.ReadAsStringAsync(ct);

            var token =
                JsonSerializer.Deserialize<GoogleAccessTokenResponse>(tokenJson)
                ?? throw new Exception("Google token response is null");

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://openidconnect.googleapis.com/v1/userinfo"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token.AccessToken
            );

            var userResponse = await httpClient.SendAsync(request, ct);
            var userJson = await userResponse.Content.ReadAsStringAsync(ct);

            var user =
                JsonSerializer.Deserialize<GoogleUserResponse>(userJson)
                ?? throw new Exception("Google user response is null");

            if (!user.EmailVerified)
            {
                return Result<OAuthUser>.Failure(
                    Error.Conflict("Google user email is not verified")
                );
            }

            var providerIdResult = ProviderId.Create(user.Sub);
            if (providerIdResult.IsFailure)
            {
                return Result<OAuthUser>.Failure(providerIdResult.Error);
            }

            var emailResult = Email.Create(user.Email);
            if (emailResult.IsFailure)
            {
                return Result<OAuthUser>.Failure(emailResult.Error);
            }

            return Result<OAuthUser>.Success(
                new OAuthUser(providerIdResult.Value, emailResult.Value)
            );
        }
        catch (Exception ex)
        {
            throw new Exception($"Google OAuth failed: {ex.Message}", ex);
        }
    }
}
