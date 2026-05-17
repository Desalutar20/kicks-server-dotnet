using Application.Admin.Products.ProductSkus.Constants;
using Domain.Product.ProductSku;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Products.Dto;
using Presentation.Admin.Products.ProductSkus.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.ProductSku;

public class CreateProductSkuTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnCreated_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var getProductsResponse = await GetProducts(null, sessionCookie, ct);
        getProductsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await getProductsResponse.Content.ReadFromJsonAsync<
            ApiCursorResponse<AdminProductDto>
        >(ct);

        var createProductSkuRequest = TestData.CreateProductSkuRequest();

        var response = await CreateProductSku(
            products!.Data[0].Id,
            createProductSkuRequest,
            sessionCookie,
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var product = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>(ct);

        var productFromDb = await GetProductSkuFromDbById(new ProductSkuId(product!.Data), ct);
        productFromDb.Should().NotBeNull();
        productFromDb.ProductSkuImages.Count.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        CreateProductSkuRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var getProductsResponse = await GetProducts(null, sessionCookie, ct);
        getProductsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await getProductsResponse.Content.ReadFromJsonAsync<
            ApiCursorResponse<AdminProductDto>
        >(ct);

        var response = await CreateProductSku(
            products!.Data[0].Id,
            invalidRequest,
            sessionCookie,
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductSkuAlreadyExists()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var getProductsResponse = await GetProducts(null, sessionCookie, ct);
        getProductsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await getProductsResponse.Content.ReadFromJsonAsync<
            ApiCursorResponse<AdminProductDto>
        >(ct);

        var createProductSkuRequest = TestData.CreateProductSkuRequest();
        var productId = products!.Data[0].Id;

        var response = await CreateProductSku(
            productId,
            createProductSkuRequest,
            sessionCookie,
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await CreateProductSku(
            products.Data[0].Id,
            createProductSkuRequest,
            sessionCookie,
            ct
        );
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = TestData.CreateProductSkuRequest();

        var response = await CreateProductSku(Guid.NewGuid(), request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var createCategoryRequest = TestData.CreateProductSkuRequest();

        var response = await CreateProductSku(
            Guid.NewGuid(),
            createCategoryRequest,
            sessionCookie,
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, CreateProductSkuRequest> InvalidRequests()
    {
        var request = TestData.CreateProductSkuRequest();

        return
        [
            ("price", request with { Price = 0 }),
            ("price", request with { Price = -1 }),
            ("salePrice", request with { SalePrice = 0 }),
            ("salePrice", request with { SalePrice = -1 }),
            ("salePrice", request with { Price = 100, SalePrice = 150 }),
            ("quantity", request with { Quantity = 0 }),
            ("quantity", request with { Quantity = -1 }),
            ("size", request with { Size = 0 }),
            ("size", request with { Size = -1 }),
            ("color", request with { Color = "  " }),
            ("color", request with { Color = "not hex" }),
            ("sku", request with { Sku = "  " }),
            ("sku", request with { Sku = TestData.String(ProductSkuSku.MaxLength + 1) }),
            (
                "images",
                request with
                {
                    Images = TestData.CreateImages(
                        Domain.Product.ProductSku.ProductSku.MaxImages + 1
                    ),
                }
            ),
            (
                "images",
                request with
                {
                    Images = TestData.CreateImages(
                        fileSizeBytes: ProductSkusConstants.MaxFileSizeBytes + 1
                    ),
                }
            ),
        ];
    }
}
