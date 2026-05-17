using Application.Admin.Products.ProductSkus.Constants;
using Domain.Product.ProductSku;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Products.ProductSkus.Dto;
using Presentation.Admin.Products.ProductSkus.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.ProductSku;

public class UpdateProductSkuTests(ApiFactory factory) : TestApp(factory)
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

        var newSku = TestData.String(ProductSkuSku.MaxLength);
        var updateProductSkuRequest = new UpdateProductSkuRequest { Sku = newSku };
        var updateProductResponse = await UpdateProductSku(
            body.Data[0].Id,
            updateProductSkuRequest,
            sessionCookie,
            ct
        );
        updateProductResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var productSku = await GetProductSkuFromDbById(new ProductSkuId(body.Data[0].Id), ct);
        productSku.Should().NotBeNull();
        productSku.Sku.Value.Should().Be(newSku);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        UpdateProductSkuRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await UpdateProductSku(Guid.Empty, invalidRequest, sessionCookie, ct);
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

        var response = await GetProductSkus(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<AdminProductSkuDto>>(
            ct
        );
        body.Should().NotBeNull();
        var productGroup = body.Data.GroupBy(x => x.Product.Id).First(g => g.Count() >= 2).ToList();

        for (var i = 0; i < 2; i++)
        {
            var updateProductSkuRequest = i switch
            {
                0 => new UpdateProductSkuRequest { Sku = productGroup[1].Sku },
                _ => new UpdateProductSkuRequest
                {
                    Color = productGroup[1].Color,
                    Size = productGroup[1].Size,
                },
            };

            var updateProductSkuResponse = await UpdateProductSku(
                productGroup[0].Id,
                updateProductSkuRequest,
                sessionCookie,
                ct
            );

            updateProductSkuResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductSkuNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var updateProductSkuRequest = new UpdateProductSkuRequest { Price = 100 };
        var updateProductSkuResponse = await UpdateProductSku(
            Guid.NewGuid(),
            updateProductSkuRequest,
            sessionCookie,
            ct
        );

        updateProductSkuResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new UpdateProductSkuRequest() { Price = 100 };
        var response = await UpdateProductSku(Guid.Empty, request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var updateProductSkuRequest = new UpdateProductSkuRequest() { Price = 100 };

        var response = await UpdateProductSku(
            Guid.Empty,
            updateProductSkuRequest,
            sessionCookie,
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, UpdateProductSkuRequest> InvalidRequests()
    {
        var request = new UpdateProductSkuRequest();

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
