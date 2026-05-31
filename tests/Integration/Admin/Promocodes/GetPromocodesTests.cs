using Application.Admin.Promocodes.Constants;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Promocodes.Dto;
using Presentation.Admin.Promocodes.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Promocodes;

public class GetPromocodesTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetPromocodes(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<AdminPromocodeDto>>(
            ct
        );

        body.Should().NotBeNull();
        body.Data.Should().HaveCount(PromocodesConstants.GetPromocodesDefaultLimit);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        GetPromocodesRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetPromocodes(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new GetPromocodesRequest(null, null, null, null);

        var response = await GetPromocodes(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var getAdminUsersRequest = new GetPromocodesRequest(null, null, null, null);
        var response = await GetPromocodes(getAdminUsersRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, GetPromocodesRequest> InvalidRequests()
    {
        var request = new GetPromocodesRequest(null, null, null, null);

        return
        [
            ("code", request with { Code = "" }),
            ("code", request with { Code = "  " }),
            ("limit", request with { Limit = 0 }),
            ("limit", request with { Limit = PromocodesConstants.GetPromocodesMaxLimit + 1 }),
            (
                "prevCursor",
                request with
                {
                    PrevCursor = TestData.String(
                        PromocodesConstants.GetPromocodesCursorMaxLength + 1
                    ),
                }
            ),
            (
                "nextCursor",
                request with
                {
                    NextCursor = TestData.String(
                        PromocodesConstants.GetPromocodesCursorMaxLength + 1
                    ),
                }
            ),
            ("prevCursor", request with { PrevCursor = "prev", NextCursor = "next" }),
        ];
    }
}
