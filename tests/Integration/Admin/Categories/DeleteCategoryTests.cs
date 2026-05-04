using Domain.Product.Category;
using Presentation.Admin.Categories.Dto;
using Presentation.Shared.Dto;

namespace Integration.Admin.Categories;

public class DeleteCategoryTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetCategories(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<CategoryDto>>(ct);
        body.Should().NotBeNull();

        var toggleBanUserResponse = await DeleteCategory(body.Data[0].Id, sessionCookie, ct);
        toggleBanUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var dbUser = await GetCategoryFromDbById(new CategoryId(body.Data[0].Id), ct);
        dbUser.Should().BeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_CategoryNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await DeleteCategory(new CategoryId(Guid.NewGuid()), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await DeleteCategory(new UserId(Guid.Empty), null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await DeleteCategory(new UserId(Guid.Empty), sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
