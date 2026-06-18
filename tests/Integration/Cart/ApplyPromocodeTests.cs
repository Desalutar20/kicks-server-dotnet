using Domain.Promocodes;
using Microsoft.AspNetCore.Mvc;
using Presentation.Cart.Dto;
using Presentation.Cart.Endpoints;
using Presentation.ProductSkus.Dto;
using Presentation.Shared.Dto;

namespace Integration.Cart;

public class RemovePromocodeTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetProductSkus(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuDto>>(ct);
        body.Should().NotBeNull();

        var addCartItemResponse = await AddCartItem(
            new AddCartItemRequest(body.Data[0].Id.ToString(), 5),
            sessionCookie,
            ct
        );
        addCartItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var promocode = await GetPromocodeFromDb(ct);

        var applyPromocodeResponse = await ApplyPromocode(
            new ApplyPromocodeRequest(promocode.Code.Value),
            sessionCookie,
            ct
        );
        applyPromocodeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getCartResponse = await GetCart(sessionCookie, ct);
        getCartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cart = await getCartResponse.Content.ReadFromJsonAsync<ApiResponse<CartDto>>(ct);

        cart.Should().NotBeNull();
        cart.Data.Items.Count.Should().Be(1);
        cart.Data.Promocode.Should().NotBeNull();
        cart.Data.Promocode.Code.Should().BeEquivalentTo(promocode.Code.Value);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        ApplyPromocodeRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await ApplyPromocode(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_PromocodeNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetProductSkus(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuDto>>(ct);
        body.Should().NotBeNull();

        var addCartItemResponse = await AddCartItem(
            new AddCartItemRequest(body.Data[0].Id.ToString(), 5),
            sessionCookie,
            ct
        );
        addCartItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var applyPromocodeResponse = await ApplyPromocode(
            new ApplyPromocodeRequest(TestData.String(PromocodeCode.MaxLength)),
            sessionCookie,
            ct
        );
        applyPromocodeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_CartIsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var promocode = await GetPromocodeFromDb(ct);

        var applyPromocodeResponse = await ApplyPromocode(
            new ApplyPromocodeRequest(promocode.Code.Value),
            sessionCookie,
            ct
        );
        applyPromocodeResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_CartAlreadyHasPromocode()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await GetProductSkus(null, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuDto>>(ct);
        body.Should().NotBeNull();

        var addCartItemResponse = await AddCartItem(
            new AddCartItemRequest(body.Data[0].Id.ToString(), 5),
            sessionCookie,
            ct
        );
        addCartItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var promocode = await GetPromocodeFromDb(ct);

        var applyPromocodeResponse = await ApplyPromocode(
            new ApplyPromocodeRequest(promocode.Code.Value),
            sessionCookie,
            ct
        );
        applyPromocodeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var applyPromocodeResponse2 = await ApplyPromocode(
            new ApplyPromocodeRequest(promocode.Code.Value),
            sessionCookie,
            ct
        );
        applyPromocodeResponse2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new AddCartItemRequest("", 5);

        var response = await AddCartItem(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public static TheoryData<string, ApplyPromocodeRequest> InvalidRequests() =>
        [("code", new ApplyPromocodeRequest("")), ("code", new ApplyPromocodeRequest("   "))];
}
