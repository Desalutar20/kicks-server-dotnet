using Presentation.Admin.Users.Dto;
using Presentation.Shared.Dto;

namespace Integration.Admin.Users;

public class ToggleBanUserTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetAdminUsers(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<AdminUserDto>>(ct);
        body.Should().NotBeNull();

        var toggleBanUserResponse = await ToggleBanUser(body.Data[0].Id, sessionCookie, ct);
        toggleBanUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbUser = await GetUserFromDbById(new UserId(body.Data[0].Id), ct);
        dbUser.Should().NotBeNull();
        dbUser.IsBanned.Should().BeTrue();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_UserNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await ToggleBanUser(new UserId(Guid.NewGuid()), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await ToggleBanUser(new UserId(Guid.Empty), null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await ToggleBanUser(new UserId(Guid.Empty), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
