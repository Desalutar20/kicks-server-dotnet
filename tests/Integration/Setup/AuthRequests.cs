using Microsoft.EntityFrameworkCore;

namespace Integration.Setup;

public partial class TestApp
{
    protected async Task<HttpResponseMessage> SignUp(
        SignUpRequest data,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/auth/sign-up", null, ct);

    protected async Task<HttpResponseMessage> SignIn(
        SignInRequest data,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/auth/sign-in", null, ct);

    protected async Task<HttpResponseMessage> VerifyAccount(
        VerifyAccountRequest data,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/auth/verify-account", null, ct);

    protected async Task<HttpResponseMessage> ForgotPassword(
        ForgotPasswordRequest data,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/auth/forgot-password", null, ct);

    protected async Task<HttpResponseMessage> ResetPassword(
        ResetPasswordRequest data,
        CancellationToken ct = default
    ) => await Request(data, HttpMethod.Post, "/api/v1/auth/reset-password", null, ct);

    protected async Task<HttpResponseMessage> GetProfile(
        string? cookie,
        CancellationToken ct = default
    ) => await Request("/api/v1/auth/profile", cookie, ct);

    protected async Task<HttpResponseMessage> Logout(
        string? cookie,
        CancellationToken ct = default
    ) => await Request<string?>(null, HttpMethod.Post, "/api/v1/auth/logout", cookie, ct);

    protected async Task CreateAndVerify(SignUpRequest data, CancellationToken ct = default)
    {
        var signUpResponse = await SignUp(data, ct);
        signUpResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var token = await GetRedisToken(TokenType.AccountVerification, ct);
        token.Should().NotBeNull();

        var accountVerificationResponse = await VerifyAccount(
            new VerifyAccountRequest(token!, data.Email),
            ct
        );
        accountVerificationResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    protected async Task<string> CreateAndSignIn(
        SignUpRequest data,
        CancellationToken ct = default,
        Role? role = null
    )
    {
        await CreateAndVerify(data, ct);

        if (role is not null)
        {
            await _dbContext
                .Users.Where(u => u.Email == data.Email)
                .ExecuteUpdateAsync(u => u.SetProperty(user => user.Role, user => role.Value), ct);
        }

        var signInResponse = await SignIn(
            new SignInRequest(data.Email, data.Password),
            TestContext.Current.CancellationToken
        );
        signInResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var ok = signInResponse.Headers.TryGetValues("Set-Cookie", out var values);
        ok.Should().BeTrue();

        var sessionCookie = values!.FirstOrDefault(c =>
            c.StartsWith(_config.Application.SessionCookieName)
        );
        sessionCookie.Should().NotBeNull();

        return sessionCookie!;
    }
}
