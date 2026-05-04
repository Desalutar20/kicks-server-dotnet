using Domain.Product.Brand;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Brands.Dto;
using Presentation.Admin.Brands.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Brands;

public class UpdateBrandTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetBrands(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<BrandDto>>(ct);
        body.Should().NotBeNull();

        var newName = TestData.String(BrandName.MaxLength);
        var updateBrandRequest = new UpdateBrandRequest(newName);
        var updateBrandResponse = await UpdateBrand(
            updateBrandRequest,
            body.Data[0].Id,
            sessionCookie,
            ct
        );
        updateBrandResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var brand = await GetBrandFromDbById(new BrandId(body.Data[0].Id), ct);
        brand.Should().NotBeNull();
        brand.Name.Value.Should().Be(newName);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        UpdateBrandRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await UpdateBrand(invalidRequest, Guid.Empty, sessionCookie, ct);
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

        var response = await GetBrands(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<BrandDto>>(ct);
        body.Should().NotBeNull();

        var updateBrandRequest = new UpdateBrandRequest(body.Data[1].Name);
        var updateBrandResponse = await UpdateBrand(
            updateBrandRequest,
            body.Data[0].Id,
            sessionCookie,
            ct
        );

        updateBrandResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_BrandNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var updateBrandRequest = new UpdateBrandRequest(TestData.String(BrandName.MaxLength));
        var updateBrandResponse = await UpdateBrand(
            updateBrandRequest,
            Guid.NewGuid(),
            sessionCookie,
            ct
        );

        updateBrandResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new UpdateBrandRequest("");

        var response = await UpdateBrand(request, Guid.Empty, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var createBrandRequest = new UpdateBrandRequest("");
        var response = await UpdateBrand(createBrandRequest, Guid.Empty, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, UpdateBrandRequest> InvalidRequests()
    {
        return
        [
            ("name", new UpdateBrandRequest("")),
            ("name", new UpdateBrandRequest(TestData.String(BrandName.MaxLength + 1))),
        ];
    }
}
