using Application.Admin.DeliveryOptions.Types;
using Presentation.Shared.Dto;

namespace Integration.Admin.DeliveryOptions;

public sealed class GetAdminDeliveryOptionsTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetAdminDeliveryOptions(sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<AdminDeliveryOptionResponse>>
        >(ct);

        body.Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await GetAdminDeliveryOptions(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetAdminDeliveryOptions(sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
