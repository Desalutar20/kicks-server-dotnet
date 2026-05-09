using Domain.Product.Category;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Categories.Endpoints;

namespace Integration.Admin.Categories;

public class CreateCategoryTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnCreated_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var createCategoryRequest = new CreateCategoryRequest(
            TestData.String(CategoryName.MaxLength)
        );
        var response = await CreateCategory(createCategoryRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var brand = await GetCategoryFromDbByName(
            CategoryName.Create(createCategoryRequest.Name).Value,
            ct
        );

        brand.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        CreateCategoryRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await CreateCategory(invalidRequest, sessionCookie, ct);
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

        var createCategoryRequest = new CreateCategoryRequest(
            TestData.String(CategoryName.MaxLength)
        );
        var response = await CreateCategory(createCategoryRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await CreateCategory(createCategoryRequest, sessionCookie, ct);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new CreateCategoryRequest("");

        var response = await CreateCategory(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var createCategoryRequest = new CreateCategoryRequest("");
        var response = await CreateCategory(createCategoryRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, CreateCategoryRequest> InvalidRequests()
    {
        return
        [
            ("name", new CreateCategoryRequest("")),
            ("name", new CreateCategoryRequest("   ")),
            ("name", new CreateCategoryRequest(TestData.String(CategoryName.MaxLength + 1))),
        ];
    }
}
