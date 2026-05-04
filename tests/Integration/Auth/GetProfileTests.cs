using Presentation.Auth.Dto;
using Presentation.Shared.Dto;

namespace Integration.Auth;

public class GetProfileTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetProfile(sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(ct);

        body.Should().NotBeNull();
        body.Data.Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await GetProfile(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
