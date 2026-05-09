using Domain.Product;
using Domain.Product.Brand;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Brands.Dto;
using Presentation.Admin.Brands.Endpoints;
using Presentation.Admin.Products.Dto;
using Presentation.Admin.Products.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Products;

public class UpdateProductTests(ApiFactory factory) : TestApp(factory)
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

        var newTitle = TestData.String(ProductTitle.MaxLength);
        var updateProductRequest = new UpdateProductRequest(newTitle, null, null, null, null, null);
        var updateProductResponse = await UpdateProduct(
            updateProductRequest,
            body.Data[0].Id,
            sessionCookie,
            ct
        );
        updateProductResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await GetProductFromDbById(new ProductId(body.Data[0].Id), ct);
        product.Should().NotBeNull();
        product.Title.Value.Should().Be(newTitle);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        UpdateProductRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await UpdateProduct(invalidRequest, Guid.Empty, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductAlreadyExists()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetProducts(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductDto>>(ct);
        body.Should().NotBeNull();

        var updateProductRequest = new UpdateProductRequest(
            body.Data[1].Title,
            null,
            body.Data[1].Gender.ToString(),
            null,
            body.Data[1].BrandId.ToString(),
            body.Data[1].CategoryId.ToString()
        );
        var updateProductResponse = await UpdateProduct(
            updateProductRequest,
            body.Data[0].Id,
            sessionCookie,
            ct
        );

        updateProductResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var updateProductRequest = new UpdateProductRequest(null, null, null, null, null, null);
        var updateProductResponse = await UpdateProduct(
            updateProductRequest,
            Guid.NewGuid(),
            sessionCookie,
            ct
        );

        updateProductResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new UpdateProductRequest(null, null, null, null, null, null);

        var response = await UpdateProduct(request, Guid.Empty, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var updateProductRequest = new UpdateProductRequest(null, null, null, null, null, null);

        var response = await UpdateProduct(updateProductRequest, Guid.Empty, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, UpdateProductRequest> InvalidRequests()
    {
        var request = new UpdateProductRequest(null, null, null, null, null, null);

        return
        [
            ("title", request with { Title = "" }),
            ("title", request with { Title = "  " }),
            ("title", request with { Title = TestData.String(ProductTitle.MaxLength + 1) }),
            (
                "description",
                request with
                {
                    Description = TestData.String(ProductDescription.MaxLength + 1),
                }
            ),
            (
                "description",
                request with
                {
                    Description = TestData.String(ProductDescription.MaxLength + 1),
                }
            ),
            (
                "description",
                request with
                {
                    Description = TestData.String(ProductDescription.MaxLength + 1),
                }
            ),
            ("gender", request with { Gender = "invalid gender" }),
            (
                "tags",
                request with
                {
                    Tags = [.. Enumerable.Range(0, ProductTags.MaxTags + 1).Select(_ => "tag")],
                }
            ),
            ("brandId", request with { BrandId = "non guid" }),
            ("categoryId", request with { CategoryId = "non guid" }),
        ];
    }
}
