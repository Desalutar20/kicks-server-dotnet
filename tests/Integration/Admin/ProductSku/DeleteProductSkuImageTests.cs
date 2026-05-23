using Domain.Product.ProductSku;
using Presentation.Admin.Products.ProductSkus.Dto;
using Presentation.Shared.Dto;

namespace Integration.Admin.ProductSku;

public class DeleteProductSkuImageTests(ApiFactory factory) : TestApp(factory)
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

        var deleteProductSkuImageResponse = await DeleteProductSkuImage(
            body.Data[0].Id,
            body.Data[0].Images[0].ImageId,
            sessionCookie,
            ct
        );
        deleteProductSkuImageResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbProductSku = await GetProductSkuFromDbById(new ProductSkuId(body.Data[0].Id), ct);
        dbProductSku.Should().NotBeNull();
        dbProductSku
            .Images.Images.Should()
            .NotContain(image => image.ImageId == body.Data[0].Images[0].ImageId);
        dbProductSku.Images.Images.Should().HaveCount(body.Data[0].Images.Count - 1);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_RemovingLastProductSkuImage()
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

        for (var i = 0; i < body.Data[0].Images.Count; i++)
        {
            var deleteProductSkuImageResponse = await DeleteProductSkuImage(
                body.Data[0].Id,
                body.Data[0].Images[i].ImageId,
                sessionCookie,
                ct
            );

            deleteProductSkuImageResponse
                .StatusCode.Should()
                .Be(
                    i < body.Data[0].Images.Count - 1
                        ? HttpStatusCode.OK
                        : HttpStatusCode.BadRequest
                );
        }
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductSkuNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await DeleteProductSkuImage(
            Guid.NewGuid(),
            Guid.NewGuid(),
            sessionCookie,
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductSkuImageNotFound()
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

        var deleteProductSkuImageResponse = await DeleteProductSkuImage(
            body.Data[0].Id,
            Guid.NewGuid(),
            sessionCookie,
            ct
        );
        deleteProductSkuImageResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await DeleteProductSkuImage(Guid.Empty, Guid.Empty, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await DeleteProductSkuImage(Guid.Empty, Guid.Empty, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
