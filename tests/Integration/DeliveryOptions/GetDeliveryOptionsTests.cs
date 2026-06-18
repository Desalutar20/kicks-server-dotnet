using Presentation.DeliveryOptions.Dto;
using Presentation.Shared.Dto;

namespace Integration.DeliveryOptions;

public sealed class GetDeliveryOptionsTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetDeliveryOptions(sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<DeliveryOptionDto>>
        >(ct);

        body.Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await GetDeliveryOptions(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
