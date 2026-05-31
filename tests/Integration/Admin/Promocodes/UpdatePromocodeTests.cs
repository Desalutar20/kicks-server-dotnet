using Domain.Promocodes;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Promocodes.Dto;
using Presentation.Admin.Promocodes.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Promocodes;

public class UpdatePromocodeTests(ApiFactory factory) : TestApp(factory)
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

        var newCode = TestData.String(PromocodeCode.MaxLength);
        var updatePromocodeRequest = new UpdatePromocodeRequest(1, null, null, null, newCode);
        var updatePromocodeResponse = await UpdatePromocode(
            body.Data[0].Id,
            updatePromocodeRequest,
            sessionCookie,
            ct
        );

        updatePromocodeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var promocode = await GetPromocodeFromDbById(new PromocodeId(body.Data[0].Id), ct);
        promocode.Should().NotBeNull();
        promocode.Code.Value.Should().Be(newCode);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        UpdatePromocodeRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await UpdatePromocode(Guid.Empty, invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_PromocodeAlreadyExists()
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

        var updatePromocodeRequest = new UpdatePromocodeRequest(
            null,
            null,
            null,
            null,
            body.Data[1].Code
        );
        var updatePromocodeResponse = await UpdatePromocode(
            body.Data[0].Id,
            updatePromocodeRequest,
            sessionCookie,
            ct
        );

        updatePromocodeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_WhenPercentPromocodeHasInvalidDiscount()
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

        var promocodeWithPercentageType = body.Data.FirstOrDefault(p =>
            p.Type == PromocodeType.Percent
        );
        if (promocodeWithPercentageType is null)
            return;

        var updatePromocodeRequest = new UpdatePromocodeRequest(101, null, null, null, null);

        var updatePromocodeResponse = await UpdatePromocode(
            promocodeWithPercentageType.Id,
            updatePromocodeRequest,
            sessionCookie,
            ct
        );

        updatePromocodeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_PromocodeNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var updatePromocodeRequest = new UpdatePromocodeRequest(null, null, null, null, null);
        var updatePromocodeResponse = await UpdatePromocode(
            Guid.NewGuid(),
            updatePromocodeRequest,
            sessionCookie,
            ct
        );

        updatePromocodeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new UpdatePromocodeRequest(null, null, null, null, null);

        var response = await UpdatePromocode(Guid.Empty, request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var updatePromocodeRequest = new UpdatePromocodeRequest(null, null, null, null, null);

        var response = await UpdatePromocode(Guid.Empty, updatePromocodeRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, UpdatePromocodeRequest> InvalidRequests()
    {
        var request = new UpdatePromocodeRequest(null, null, null, null, null);

        return
        [
            ("discountValue", request with { DiscountValue = 0 }),
            ("discountValue", request with { DiscountValue = -1 }),
            (
                "discountValue",
                request with
                {
                    Type = nameof(PromocodeType.Percent),
                    DiscountValue = 101,
                }
            ),
            ("type", request with { Type = "invalid type" }),
            (
                "validityPeriod",
                request with
                {
                    ValidityPeriod = new PromocodeValidityPeriodDto(
                        DateTimeOffset.UtcNow.AddDays(1),
                        DateTimeOffset.UtcNow
                    ),
                }
            ),
            (
                "validityPeriod",
                request with
                {
                    ValidityPeriod = new PromocodeValidityPeriodDto(
                        DateTimeOffset.UtcNow.AddDays(-2),
                        DateTimeOffset.UtcNow.AddDays(-1)
                    ),
                }
            ),
            ("usageLimit", request with { UsageLimit = 0 }),
            ("usageLimit", request with { UsageLimit = -1 }),
            ("code", request with { Code = "" }),
            ("code", request with { Code = "  " }),
            ("code", request with { Code = TestData.String(PromocodeCode.MaxLength + 1) }),
        ];
    }
}
