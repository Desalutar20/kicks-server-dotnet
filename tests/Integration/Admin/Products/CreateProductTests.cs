using Domain.Product;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Brands.Dto;
using Presentation.Admin.Categories.Dto;
using Presentation.Admin.Products.Dto;
using Presentation.Admin.Products.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Products;

public class CreateProductTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnCreated_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var getCategoriesResponse = await GetCategories(null, sessionCookie, ct);
        getCategoriesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var categories = await getCategoriesResponse.Content.ReadFromJsonAsync<
            ApiCursorResponse<CategoryDto>
        >(ct);

        var getBrandsResponse = await GetBrands(null, sessionCookie, ct);
        getBrandsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var brands = await getBrandsResponse.Content.ReadFromJsonAsync<ApiCursorResponse<BrandDto>>(
            ct
        );

        var createProductRequest = TestData.CreateProductRequest(
            categories!.Data[0].Id.ToString(),
            brands!.Data[0].Id.ToString()
        );

        var response = await CreateProduct(createProductRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var product = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>(ct);

        var productFromDb = await GetProductFromDbById(new ProductId(product!.Data.Id), ct);
        productFromDb.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        CreateProductRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await CreateProduct(invalidRequest, sessionCookie, ct);
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

        var getCategoriesResponse = await GetCategories(null, sessionCookie, ct);
        getCategoriesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var categories = await getCategoriesResponse.Content.ReadFromJsonAsync<
            ApiCursorResponse<CategoryDto>
        >(ct);

        var getBrandsResponse = await GetBrands(null, sessionCookie, ct);
        getBrandsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var brands = await getBrandsResponse.Content.ReadFromJsonAsync<ApiCursorResponse<BrandDto>>(
            ct
        );

        var createProductRequest = TestData.CreateProductRequest(
            categories!.Data[0].Id.ToString(),
            brands!.Data[0].Id.ToString()
        );
        var response = await CreateProduct(createProductRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await CreateProduct(createProductRequest, sessionCookie, ct);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = TestData.CreateProductRequest("", "");

        var response = await CreateProduct(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var createCategoryRequest = TestData.CreateProductRequest("", "");

        var response = await CreateProduct(createCategoryRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, CreateProductRequest> InvalidRequests()
    {
        var request = TestData.CreateProductRequest(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString()
        );

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
