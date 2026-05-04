using Microsoft.AspNetCore.Mvc;

namespace Integration.Auth;

public class SignUpTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnCreated_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var response = await SignUp(request, ct);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var user = await GetUserFromDbByEmail(Email.Create(request.Email).Value, ct);

        user.Should().NotBeNull();
        user.IsVerified.Should().BeFalse();
        user.HashedPassword!.Value.Should().NotBeEquivalentTo(request.Password);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        SignUpRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await SignUp(invalidRequest, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    public static TheoryData<string, SignUpRequest> InvalidRequests()
    {
        var request = TestData.SignUpRequest();

        return
        [
            ("email", request with { Email = "invalid email" }),
            ("email", request with { Email = "" }),
            ("password", request with { Password = "" }),
            ("password", request with { Password = TestData.Password(Password.MinLength - 1) }),
            ("password", request with { Password = TestData.Password(Password.MaxLength + 1) }),
            ("firstName", request with { FirstName = "" }),
            ("firstName", request with { FirstName = new string('p', FirstName.MaxLength + 1) }),
            ("lastName", request with { LastName = "" }),
            ("lastName", request with { LastName = new string('p', LastName.MaxLength + 1) }),
            ("gender", request with { Gender = "invalid gender" }),
        ];
    }
}
