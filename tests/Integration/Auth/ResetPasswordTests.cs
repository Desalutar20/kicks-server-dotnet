using Microsoft.AspNetCore.Mvc;

namespace Integration.Auth;

public class ResetPasswordTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        await CreateAndVerify(request, ct);

        var forgotPasswordResponse = await ForgotPassword(
            new ForgotPasswordRequest(request.Email),
            ct
        );
        forgotPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await GetRedisToken(TokenType.ResetPassword, ct);
        token.Should().NotBeNull();

        var userBeforeUpdate = await GetUserFromDbByEmail(Email.Create(request.Email).Value, ct);
        userBeforeUpdate.Should().NotBeNull();

        var response = await ResetPassword(
            new ResetPasswordRequest(token, request.Email, TestData.Password(Password.MinLength)),
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await GetUserFromDbByEmail(Email.Create(request.Email).Value, ct);
        user!.HashedPassword.Should().NotBeEquivalentTo(userBeforeUpdate.HashedPassword);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        ResetPasswordRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await ResetPassword(invalidRequest, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_EmailIsDifferent()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        await CreateAndVerify(request, ct);

        var forgotPasswordResponse = await ForgotPassword(
            new ForgotPasswordRequest(request.Email),
            ct
        );
        forgotPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await GetRedisToken(TokenType.ResetPassword, ct);
        token.Should().NotBeNull();

        var resetPasswordResponse = await ResetPassword(
            new ResetPasswordRequest(
                token,
                TestData.Email(),
                TestData.Password(Password.MinLength)
            ),
            ct
        );
        resetPasswordResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_UserDoesNotExist()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        await CreateAndVerify(request, ct);

        var forgotPasswordResponse = await ForgotPassword(
            new ForgotPasswordRequest(request.Email),
            ct
        );
        forgotPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await GetRedisToken(TokenType.ResetPassword, ct);
        token.Should().NotBeNull();

        await DeleteUserFromDbByEmail(Email.Create(request.Email).Value, ct);

        var resetPasswordResponse = await ResetPassword(
            new ResetPasswordRequest(token, request.Email, TestData.Password(Password.MinLength)),
            ct
        );
        resetPasswordResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_UserIsBanned()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        await CreateAndVerify(request, ct);

        var forgotPasswordResponse = await ForgotPassword(
            new ForgotPasswordRequest(request.Email),
            ct
        );
        forgotPasswordResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await GetRedisToken(TokenType.ResetPassword, ct);
        token.Should().NotBeNull();

        await BanUserInDbByEmail(Email.Create(request.Email).Value, ct);

        var resetPasswordResponse = await ResetPassword(
            new ResetPasswordRequest(token, request.Email, TestData.Password(Password.MinLength)),
            ct
        );
        resetPasswordResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public static TheoryData<string, ResetPasswordRequest> InvalidRequests() =>
        [
            (
                "token",
                new ResetPasswordRequest(
                    "",
                    TestData.Email(),
                    TestData.Password(Password.MinLength)
                )
            ),
            ("email", new ResetPasswordRequest("token", "", TestData.Password(Password.MinLength))),
            (
                "email",
                new ResetPasswordRequest(
                    "token",
                    "invalid email",
                    TestData.Password(Password.MinLength)
                )
            ),
            ("newPassword", new ResetPasswordRequest("token", TestData.Email(), "")),
            (
                "newPassword",
                new ResetPasswordRequest(
                    "token",
                    TestData.Email(),
                    TestData.Password(Password.MinLength - 1)
                )
            ),
            (
                "newPassword",
                new ResetPasswordRequest(
                    "token",
                    TestData.Email(),
                    TestData.Password(Password.MaxLength + 1)
                )
            ),
        ];
}
