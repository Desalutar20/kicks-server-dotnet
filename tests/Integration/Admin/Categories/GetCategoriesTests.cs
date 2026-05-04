using Application.Admin.Categories.Constants;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Categories.Dto;
using Presentation.Admin.Categories.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Categories;

public class GetCategoriesTests(ApiFactory factory) : TestApp(factory)
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
        body.Data.Should().HaveCount(CategoriesConstants.GetCategoriesDefaultLimit);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        GetCategoriesRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetCategories(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new GetCategoriesRequest(null, null, null, null);

        var response = await GetCategories(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var getAdminUsersRequest = new GetCategoriesRequest(null, null, null, null);
        var response = await GetCategories(getAdminUsersRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, GetCategoriesRequest> InvalidRequests()
    {
        var request = new GetCategoriesRequest(null, null, null, null);

        return
        [
            (
                "search",
                request with
                {
                    Search = TestData.String(CategoriesConstants.GetCategoriesSearchMaxLength + 1),
                }
            ),
            ("limit", request with { Limit = 0 }),
            ("limit", request with { Limit = CategoriesConstants.GetCategoriesMaxLimit + 1 }),
            (
                "prevCursor",
                request with
                {
                    PrevCursor = TestData.String(
                        CategoriesConstants.GetCategoriesCursorMaxLength + 1
                    ),
                }
            ),
            (
                "nextCursor",
                request with
                {
                    NextCursor = TestData.String(
                        CategoriesConstants.GetCategoriesCursorMaxLength + 1
                    ),
                }
            ),
            ("prevCursor", request with { PrevCursor = "prev", NextCursor = "next" }),
        ];
    }
}
