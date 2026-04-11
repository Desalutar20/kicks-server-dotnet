using Microsoft.AspNetCore.Mvc;
using Presentation.Auth.Dto;
using Presentation.Shared.Dto;

namespace Integration.Auth;

public class SignInTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnCreated_WhenRequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        await CreateAndVerify(request, ct);

        var signInResponse = await SignIn(new SignInRequest(request.Email, request.Password),
            TestContext.Current.CancellationToken);
        signInResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body =
            await signInResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(
                ct);

        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
    }


    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(string field, SignInRequest invalidRequest)
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await SignIn(invalidRequest, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error!.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask SignIn_Should_ReturnBadRequest_When_UserDoesNotExist()
    {
        var request = new SignInRequest(TestData.Email(), TestData.Password(Password.MinLength));

        var response = await SignIn(request, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async ValueTask SignIn_Should_ReturnBadRequest_When_PasswordIsInvalid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        await CreateAndVerify(request, ct);

        var wrongPasswordRequest = new SignInRequest(request.Email, TestData.Password(Password.MinLength));

        var response = await SignIn(wrongPasswordRequest, ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask SignIn_Should_ReturnBadRequest_When_UserNotVerified()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();

        var signUp = await SignUp(request, ct);
        signUp.StatusCode.Should().Be(HttpStatusCode.Created);

        var signInResponse = await SignIn(
            new SignInRequest(request.Email, request.Password),
            ct);

        signInResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask SignIn_Should_ReturnBadRequest_When_UserIsBanned()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();

        await CreateAndVerify(request, ct);

        await BanUserInDbByEmail(Email.Create(request.Email).Value);

        var response = await SignIn(
            new SignInRequest(request.Email, request.Password),
            ct);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    public static TheoryData<string, SignInRequest> InvalidRequests() =>
    [
        ("email", new SignInRequest("", TestData.Password(Password.MinLength))),
        ("email", new SignInRequest("invalid email", TestData.Password(Password.MinLength))),
        ("password", new SignInRequest(TestData.Email(), "")),
        ("password", new SignInRequest(TestData.Email(), TestData.Password(Password.MinLength - 1))),
        ("password", new SignInRequest(TestData.Email(), TestData.Password(Password.MaxLength + 1)))
    ];
}