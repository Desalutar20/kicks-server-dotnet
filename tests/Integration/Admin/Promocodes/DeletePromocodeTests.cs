using Domain.Promocodes;
using Presentation.Admin.Promocodes.Dto;
using Presentation.Shared.Dto;

namespace Integration.Admin.Promocodes;

public class DeletePromocodeTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetPromocodes(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<AdminPromocodeDto>>(
            ct
        );
        body.Should().NotBeNull();

        var deletePromocodeResponse = await DeletePromocode(body.Data[0].Id, sessionCookie, ct);
        deletePromocodeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbPromocode = await GetPromocodeFromDbById(new PromocodeId(body.Data[0].Id), ct);
        dbPromocode.Should().BeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_PromocodeNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await DeletePromocode(Guid.NewGuid(), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await DeletePromocode(Guid.Empty, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await DeletePromocode(Guid.Empty, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
