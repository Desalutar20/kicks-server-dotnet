using Domain.DeliveryOptions;
using Presentation.Admin.DeliveryOptions.Dto;
using Presentation.Shared.Dto;

namespace Integration.Admin.DeliveryOptions;

public class DeleteDeliveryOptionTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetDeliveryOptions(sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<
            ApiResponse<IReadOnlyList<AdminDeliveryOptionDto>>
        >(ct);
        body.Should().NotBeNull();

        var deleteDeliveryOptionResponse = await DeleteDeliveryOption(
            body.Data[0].Id,
            sessionCookie,
            ct
        );
        deleteDeliveryOptionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbDeliveryOption = await GetDeliveryOptionFromDbById(
            new DeliveryOptionId(body.Data[0].Id),
            ct
        );
        dbDeliveryOption.Should().BeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_DeliveryOptionNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await DeleteDeliveryOption(Guid.NewGuid(), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await DeleteDeliveryOption(Guid.Empty, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await DeleteDeliveryOption(Guid.Empty, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
