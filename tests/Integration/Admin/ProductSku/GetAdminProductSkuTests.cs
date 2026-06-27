using Application.Admin.Products.ProductSkus.Types;
using Presentation.Shared.Dto;

namespace Integration.Admin.ProductSku;

public sealed class GetAdminProductSkuTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetProductSkus(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<
            ApiCursorResponse<AdminProductSkuResponse>
        >(ct);
        body.Should().NotBeNull();

        var getProductSkuResponse = await GetAdminProductSku(body.Data[0].Id, sessionCookie, ct);
        getProductSkuResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var productSku = await getProductSkuResponse.Content.ReadFromJsonAsync<
            ApiResponse<AdminProductSkuResponse>
        >(ct);
        productSku.Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductSkuNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetAdminProductSku(Guid.NewGuid(), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await GetAdminProductSku(Guid.NewGuid(), null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetAdminProductSku(Guid.NewGuid(), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
