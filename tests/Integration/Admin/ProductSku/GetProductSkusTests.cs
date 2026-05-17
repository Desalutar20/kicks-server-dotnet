using Application.Admin.Products.ProductSkus.Constants;
using Domain.Product.ProductSku;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Products.ProductSkus.Dto;
using Presentation.Admin.Products.ProductSkus.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.ProductSku;

public class GetProductSkusTests(ApiFactory factory) : TestApp(factory)
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
        body.Data.Should().HaveCount(ProductSkusConstants.GetProductSkusDefaultLimit);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        GetProductSkusRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetProductSkus(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await GetProducts(null, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetCategories(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, GetProductSkusRequest> InvalidRequests()
    {
        var request = new GetProductSkusRequest(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        return
        [
            ("minPrice", request with { MinPrice = 0 }),
            ("minPrice", request with { MinPrice = -1 }),
            ("maxPrice", request with { MaxPrice = 0 }),
            ("maxPrice", request with { MaxPrice = -1 }),
            ("minPrice", request with { MinPrice = 150, MaxPrice = 100 }),
            ("minSalePrice", request with { MinSalePrice = 0 }),
            ("minSalePrice", request with { MinSalePrice = -1 }),
            ("maxSalePrice", request with { MaxSalePrice = 0 }),
            ("maxSalePrice", request with { MaxSalePrice = -1 }),
            ("minSalePrice", request with { MinSalePrice = 150, MaxSalePrice = 100 }),
            ("size", request with { Size = 0 }),
            ("size", request with { Size = -1 }),
            ("color", request with { Color = "Invalid color" }),
            ("sku", request with { Sku = TestData.String(ProductSkuSku.MaxLength + 1) }),
            ("limit", request with { Limit = 0 }),
            ("limit", request with { Limit = -1 }),
            ("limit", request with { Limit = ProductSkusConstants.GetProductSkusMaxLimit + 1 }),
            (
                "prevCursor",
                request with
                {
                    PrevCursor = TestData.String(
                        ProductSkusConstants.GetProductSkusCursorMaxLength + 1
                    ),
                }
            ),
            (
                "nextCursor",
                request with
                {
                    NextCursor = TestData.String(
                        ProductSkusConstants.GetProductSkusCursorMaxLength + 1
                    ),
                }
            ),
            ("prevCursor", request with { PrevCursor = "prev", NextCursor = "next" }),
        ];
    }
}
