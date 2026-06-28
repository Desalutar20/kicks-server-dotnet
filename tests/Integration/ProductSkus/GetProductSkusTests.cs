using Application.Admin.Products.ProductSkus.Constants;
using Application.ProductSkus.Types;
using Microsoft.AspNetCore.Mvc;
using Presentation.ProductSkus.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.ProductSkus;

public sealed class GetProductSkusTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await GetProductSkus(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuResponse>>(
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

        var response = await GetProductSkus(invalidRequest, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
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
            null
        );

        return
        [
            ("sizes", request with { Sizes = [-1] }),
            ("sizes", request with { Sizes = [0] }),
            ("colors", request with { Colors = [""] }),
            ("colors", request with { Colors = ["  "] }),
            ("colors", request with { Colors = ["not hex"] }),
            ("categoryIds", request with { CategoryIds = ["not uuid"] }),
            ("brandIds", request with { BrandIds = ["not uuid"] }),
            ("genders", request with { Genders = ["invalid gender"] }),
            ("minPrice", request with { MinPrice = 0 }),
            ("minPrice", request with { MinPrice = -1 }),
            ("maxPrice", request with { MaxPrice = 0 }),
            ("maxPrice", request with { MaxPrice = -1 }),
            ("minPrice", request with { MinPrice = 150, MaxPrice = 100 }),
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
