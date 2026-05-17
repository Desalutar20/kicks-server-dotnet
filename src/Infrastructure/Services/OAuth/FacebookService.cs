using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.OAuth;

namespace Infrastructure.Services.OAuth;

internal sealed record FacebookAccessTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken
);

internal sealed record FacebookUserResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("email")] string Email
);

internal sealed class FacebookService(HttpClient httpClient, Config config) : IOAuthClient
{
    private OAuthProviderConfig Cfg => config.OAuth.Facebook;

    public Uri GenerateRedirectUrl(OAuthState state) =>
        new Uri("https://www.facebook.com/v20.0/dialog/oauth")
            .AddParameter("client_id", Cfg.ClientId)
            .AddParameter("redirect_uri", Cfg.RedirectUrl)
            .AddParameter("response_type", "code")
            .AddParameter("scope", "email")
            .AddParameter("state", state.ToString());

    public bool IsValidState(OAuthState received, OAuthState expected) => received == expected;

    public async Task<Result<OAuthUser>> GetUserAsync(
        NonEmptyString code,
        CancellationToken ct = default
    )
    {
        try
        {
            var tokenRequestUri = new Uri("https://graph.facebook.com/v20.0/oauth/access_token")
                .AddParameter("code", code.Value)
                .AddParameter("client_id", Cfg.ClientId)
                .AddParameter("client_secret", Cfg.ClientSecret)
                .AddParameter("redirect_uri", Cfg.RedirectUrl);

            var tokenResponse = await httpClient.GetAsync(tokenRequestUri, ct);
            var tokenJson = await tokenResponse.Content.ReadAsStringAsync(ct);

            var token =
                JsonSerializer.Deserialize<FacebookAccessTokenResponse>(tokenJson)
                ?? throw new Exception("Facebook token response is null");

            var userResponse = await httpClient.GetAsync(
                new Uri("https://graph.facebook.com/v20.0/me")
                    .AddParameter("fields", "id,first_name,last_name,gender,email")
                    .AddParameter("access_token", token.AccessToken),
                ct
            );

            var userJson = await userResponse.Content.ReadAsStringAsync(ct);
            var user =
                JsonSerializer.Deserialize<FacebookUserResponse>(userJson)
                ?? throw new Exception("Facebook user response is null");

            var providerIdResult = ProviderId.Create(user.Id);
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
            throw new Exception($"Facebook OAuth failed: {ex.Message}", ex);
        }
    }
}
