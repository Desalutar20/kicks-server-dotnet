using Application.Admin.Brands.Constants;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Brands.Dto;
using Presentation.Admin.Brands.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Brands;

public class GetBrandsTests(ApiFactory factory) : TestApp(factory)
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
        body.Data.Should().HaveCount(BrandsConstants.GetBrandsDefaultLimit);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        GetBrandsRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetBrands(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new GetBrandsRequest(null, null, null, null);

        var response = await GetBrands(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var getAdminUsersRequest = new GetBrandsRequest(null, null, null, null);
        var response = await GetBrands(getAdminUsersRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, GetBrandsRequest> InvalidRequests()
    {
        var request = new GetBrandsRequest(null, null, null, null);

        return
        [
            (
                "search",
                request with
                {
                    Search = TestData.String(BrandsConstants.GetBrandsSearchMaxLength + 1),
                }
            ),
            ("limit", request with { Limit = 0 }),
            ("limit", request with { Limit = BrandsConstants.GetBrandsMaxLimit + 1 }),
            (
                "prevCursor",
                request with
                {
                    PrevCursor = TestData.String(BrandsConstants.GetBrandsCursorMaxLength + 1),
                }
            ),
            (
                "nextCursor",
                request with
                {
                    NextCursor = TestData.String(BrandsConstants.GetBrandsCursorMaxLength + 1),
                }
            ),
            ("prevCursor", request with { PrevCursor = "prev", NextCursor = "next" }),
        ];
    }
}
