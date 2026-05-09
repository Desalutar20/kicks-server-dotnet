using Application.Admin.Products.Constants;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Products.Dto;
using Presentation.Admin.Products.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Products;

public class GetProductsTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetProducts(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductDto>>(ct);

        body.Should().NotBeNull();
        body.Data.Should().HaveCount(ProductsConstants.GetProductsDefaultLimit);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        GetProductsRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetProducts(invalidRequest, sessionCookie, ct);
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

    public static TheoryData<string, GetProductsRequest> InvalidRequests()
    {
        var request = new GetProductsRequest(null, null, null, null, null, null, null, null);

        return
        [
            (
                "search",
                request with
                {
                    Search = TestData.String(ProductsConstants.GetProductsSearchMaxLength + 1),
                }
            ),
            ("gender", request with { Gender = "Invalid gender" }),
            ("brandId", request with { BrandId = "Non uuid" }),
            ("categoryId", request with { CategoryId = "Non uuid" }),
            ("limit", request with { Limit = 0 }),
            ("limit", request with { Limit = ProductsConstants.GetProductsMaxLimit + 1 }),
            (
                "prevCursor",
                request with
                {
                    PrevCursor = TestData.String(ProductsConstants.GetProductsCursorMaxLength + 1),
                }
            ),
            (
                "nextCursor",
                request with
                {
                    NextCursor = TestData.String(ProductsConstants.GetProductsCursorMaxLength + 1),
                }
            ),
            ("prevCursor", request with { PrevCursor = "prev", NextCursor = "next" }),
        ];
    }
}
