using Domain.Promocodes;
using Microsoft.AspNetCore.Mvc;
using Presentation.Admin.Promocodes.Dto;
using Presentation.Admin.Promocodes.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Admin.Promocodes;

public class CreatePromocodeTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnCreated_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var createPromocodeRequest = TestData.CreatePromocodeRequest();

        var response = await CreatePromocode(createPromocodeRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var promocode = await response.Content.ReadFromJsonAsync<ApiResponse<AdminPromocodeDto>>(
            ct
        );

        var promocodeFromDb = await GetPromocodeFromDbById(new PromocodeId(promocode!.Data.Id), ct);
        promocodeFromDb.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        CreatePromocodeRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await CreatePromocode(invalidRequest, sessionCookie, ct);
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

        var createPromocodeRequest = TestData.CreatePromocodeRequest();
        var response = await CreatePromocode(createPromocodeRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await CreatePromocode(createPromocodeRequest, sessionCookie, ct);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = TestData.CreatePromocodeRequest();

        var response = await CreatePromocode(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async ValueTask Should_ReturnForbidden_When_UserIsNotAdmin()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var createPromocodeRequest = TestData.CreatePromocodeRequest();

        var response = await CreatePromocode(createPromocodeRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    public static TheoryData<string, CreatePromocodeRequest> InvalidRequests()
    {
        var request = TestData.CreatePromocodeRequest();

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
