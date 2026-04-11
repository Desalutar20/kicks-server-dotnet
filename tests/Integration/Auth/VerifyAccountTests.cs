using Microsoft.AspNetCore.Mvc;
using Presentation.Auth.Dto;
using Presentation.Shared.Dto;

namespace Integration.Auth;

public class VerifyAccountTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async Task Should_ReturnOk_WhenRequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var response = await SignUp(request, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var token = await GetRedisToken(TokenType.AccountVerification, ct);
        token.Should().NotBeNull();

        var accountVerificationResponse =
            await VerifyAccount(new VerifyAccountRequest(token!, request.Email), ct);
        accountVerificationResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body =
            await accountVerificationResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(
                ct);

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();

        var user = await GetUserFromDbByEmail(Email.Create(request.Email).Value);
        user.Should().NotBeNull();
        user.IsVerified.Should().BeTrue();
    }


    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(string field,
        VerifyAccountRequest invalidRequest)
    {
        var ct = TestContext.Current.CancellationToken;

        var response =
            await VerifyAccount(invalidRequest, TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error!.Errors[field].Should().NotBeNull();
    }


    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_UserDoesNotExist()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var response = await SignUp(request, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var token = await GetRedisToken(TokenType.AccountVerification, ct);
        token.Should().NotBeNull();

        await DeleteUserFromDbByEmail(Email.Create(request.Email).Value);

        var accountVerificationResponse =
            await VerifyAccount(new VerifyAccountRequest(token!, request.Email), ct);
        accountVerificationResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_TokenDoesNotExist()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var response = await SignUp(request, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var token = await GetRedisToken(TokenType.AccountVerification, ct);
        token.Should().NotBeNull();

        await DeleteRedisAccountVerificationToken(TokenType.AccountVerification, ct);

        var accountVerificationResponse =
            await VerifyAccount(new VerifyAccountRequest(token!, request.Email), ct);
        accountVerificationResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_EmailIsDifferent()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var response = await SignUp(request, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var token = await GetRedisToken(TokenType.AccountVerification, ct);
        token.Should().NotBeNull();

        var faker = new Faker();
        faker.Internet.Email();

        var accountVerificationResponse =
            await VerifyAccount(new VerifyAccountRequest(token!, TestData.Email()), ct);
        accountVerificationResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public static TheoryData<string, VerifyAccountRequest> InvalidRequests() =>
    [
        ("email", new VerifyAccountRequest("token", "")),
        ("email", new VerifyAccountRequest("token", "invalid email")),
        ("token", new VerifyAccountRequest("", TestData.Email()))
    ];
}