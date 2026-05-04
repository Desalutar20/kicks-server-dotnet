using Microsoft.AspNetCore.Mvc;

namespace Integration.Auth;

public class ForgotPasswordTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        await CreateAndVerify(request, ct);

        var response = await ForgotPassword(new ForgotPasswordRequest(request.Email), ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid email")]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(string email)
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        await CreateAndVerify(request, ct);

        var response = await ForgotPassword(new ForgotPasswordRequest(email), ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors["email"].Should().NotBeNull();
    }
}
