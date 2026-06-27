using Application.Admin.Products.Types;
using Domain.Products;
using Presentation.Shared.Dto;

namespace Integration.Admin.Products;

public sealed class ToggleProductIsDeletedTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetProducts(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<
            ApiCursorResponse<AdminProductResponse>
        >(ct);
        body.Should().NotBeNull();

        var toggleBanUserResponse = await ToggleProductIsDeleted(
            body.Data[0].Id,
            sessionCookie,
            ct
        );
        toggleBanUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbProduct = await GetProductFromDbById(new ProductId(body.Data[0].Id), ct);

        dbProduct.Should().NotBeNull();
        dbProduct.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await ToggleProductIsDeleted(Guid.NewGuid(), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await ToggleProductIsDeleted(Guid.Empty, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await ToggleProductIsDeleted(Guid.Empty, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
