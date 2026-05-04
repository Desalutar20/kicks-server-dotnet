using Domain.Product.Category;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Categories.Dto;
using Presentation.Admin.Categories.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Categories;

public class UpdateCategoryTests(ApiFactory factory) : TestApp(factory)
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

        var newName = TestData.String(CategoryName.MaxLength);
        var updateCategoryRequest = new UpdateCategoryRequest(newName);
        var updateCategoryResponse = await UpdateCategory(
            updateCategoryRequest,
            body.Data[0].Id,
            sessionCookie,
            ct
        );
        updateCategoryResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var category = await GetCategoryFromDbById(new CategoryId(body.Data[0].Id), ct);
        category.Should().NotBeNull();
        category.Name.Value.Should().Be(newName);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        UpdateCategoryRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await UpdateCategory(invalidRequest, Guid.Empty, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_CategoryAlreadyExists()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetCategories(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<CategoryDto>>(ct);
        body.Should().NotBeNull();

        var updateCategoryRequest = new UpdateCategoryRequest(body.Data[1].Name);
        var updateCategoryResponse = await UpdateCategory(
            updateCategoryRequest,
            body.Data[0].Id,
            sessionCookie,
            ct
        );

        updateCategoryResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_CategoryNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var updateCategoryRequest = new UpdateCategoryRequest(
            TestData.String(CategoryName.MaxLength)
        );
        var updateCategoryResponse = await UpdateCategory(
            updateCategoryRequest,
            Guid.NewGuid(),
            sessionCookie,
            ct
        );

        updateCategoryResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new UpdateCategoryRequest("");

        var response = await UpdateCategory(request, Guid.Empty, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var createCategoryRequest = new UpdateCategoryRequest("");
        var response = await UpdateCategory(createCategoryRequest, Guid.Empty, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, UpdateCategoryRequest> InvalidRequests()
    {
        return
        [
            ("name", new UpdateCategoryRequest("")),
            ("name", new UpdateCategoryRequest(TestData.String(CategoryName.MaxLength + 1))),
        ];
    }
}
