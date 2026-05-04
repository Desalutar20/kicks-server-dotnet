using Application.Admin.Users.Constants;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Users.Dto;
using Presentation.Admin.Users.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Users;

public class GetAdminUsersTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetAdminUsers(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<AdminUserDto>>(ct);

        body.Should().NotBeNull();
        body.Data.Should().HaveCount(AdminUsersConstants.GetAdminUsersDefaultLimit);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        GetAdminUsersRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetAdminUsers(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new GetAdminUsersRequest(null, null, null, null, null, null, null);

        var response = await GetAdminUsers(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var getAdminUsersRequest = new GetAdminUsersRequest(
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        var response = await GetAdminUsers(getAdminUsersRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, GetAdminUsersRequest> InvalidRequests()
    {
        var request = new GetAdminUsersRequest(null, null, null, null, null, null, null);

        return
        [
            (
                "search",
                request with
                {
                    Search = TestData.String(AdminUsersConstants.GetAdminUsersSearchMaxLength + 1),
                }
            ),
            ("gender", request with { Gender = "invalid gender" }),
            ("limit", request with { Limit = 0 }),
            ("limit", request with { Limit = AdminUsersConstants.GetAdminUsersMaxLimit + 1 }),
            (
                "prevCursor",
                request with
                {
                    PrevCursor = TestData.String(
                        AdminUsersConstants.GetAdminUsersCursorMaxLength + 1
                    ),
                }
            ),
            (
                "nextCursor",
                request with
                {
                    NextCursor = TestData.String(
                        AdminUsersConstants.GetAdminUsersCursorMaxLength + 1
                    ),
                }
            ),
            ("prevCursor", request with { PrevCursor = "prev", NextCursor = "next" }),
        ];
    }
}
