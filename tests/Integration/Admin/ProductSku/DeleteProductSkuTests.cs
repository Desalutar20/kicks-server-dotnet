using Domain.Product.ProductSku;
using Presentation.Admin.Products.ProductSkus.Dto;
using Presentation.Shared.Dto;

namespace Integration.Admin.ProductSku;

public class DeleteProductSkuTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetProductSkus(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<AdminProductSkuDto>>(
            ct
        );
        body.Should().NotBeNull();

        var deleteProductSkuResponse = await DeleteProductSku(body.Data[0].Id, sessionCookie, ct);
        deleteProductSkuResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbUser = await GetProductSkuFromDbById(new ProductSkuId(body.Data[0].Id), ct);
        dbUser.Should().BeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductSkuNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await DeleteProductSku(Guid.NewGuid(), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await DeleteProductSku(Guid.Empty, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await DeleteProductSku(Guid.Empty, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
