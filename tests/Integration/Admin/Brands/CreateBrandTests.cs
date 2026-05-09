using Domain.Product.Brand;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Brands.Endpoints;

namespace Integration.Admin.Brands;

public class CreateBrandTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnCreated_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var createBrandRequest = new CreateBrandRequest(TestData.String(BrandName.MaxLength));
        var response = await CreateBrand(createBrandRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var brand = await GetBrandFromDbByName(BrandName.Create(createBrandRequest.Name).Value, ct);

        brand.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        CreateBrandRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await CreateBrand(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_BrandAlreadyExists()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var createBrandRequest = new CreateBrandRequest(TestData.String(BrandName.MaxLength));
        var response = await CreateBrand(createBrandRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await CreateBrand(createBrandRequest, sessionCookie, ct);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new CreateBrandRequest("");

        var response = await CreateBrand(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var createBrandRequest = new CreateBrandRequest("");
        var response = await CreateBrand(createBrandRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, CreateBrandRequest> InvalidRequests()
    {
        return
        [
            ("name", new CreateBrandRequest("")),
            ("name", new CreateBrandRequest("   ")),
            ("name", new CreateBrandRequest(TestData.String(BrandName.MaxLength + 1))),
        ];
    }
}
