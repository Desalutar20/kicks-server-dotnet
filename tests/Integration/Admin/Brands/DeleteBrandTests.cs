using Domain.Product.Brand;
using Presentation.Admin.Brands.Dto;
using Presentation.Shared.Dto;

namespace Integration.Admin.Brands;

public class DeleteBrandTests(ApiFactory factory) : TestApp(factory)
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

        var toggleBanUserResponse = await DeleteBrand(body.Data[0].Id, sessionCookie, ct);
        toggleBanUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbUser = await GetBrandFromDbById(new BrandId(body.Data[0].Id), ct);
        dbUser.Should().BeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_BrandNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await DeleteBrand(new BrandId(Guid.NewGuid()), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await DeleteBrand(new UserId(Guid.Empty), null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await DeleteBrand(new UserId(Guid.Empty), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
