using Application.Carts.Types;
using Application.ProductSkus.Types;
using Presentation.Cart.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Cart;

public sealed class ApplyPromocodeTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetProductSkus(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuResponse>>(
            ct
        );
        body.Should().NotBeNull();

        var addCartItemResponse = await AddCartItem(body.Data[0].Id, sessionCookie, ct);
        addCartItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var promocode = await GetPromocodeFromDb(ct);

        var applyPromocodeResponse = await ApplyPromocode(
            new ApplyPromocodeRequest(promocode.Code.Value),
            sessionCookie,
            ct
        );
        applyPromocodeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var removePromocodeResponse = await RemovePromocode(sessionCookie, ct);
        removePromocodeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getCartResponse = await GetCart(sessionCookie, ct);
        getCartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cart = await getCartResponse.Content.ReadFromJsonAsync<ApiResponse<CartResponse>>(ct);

        cart.Should().NotBeNull();
        cart.Data.Items.Count.Should().Be(1);
        cart.Data.Promocode.Should().BeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await RemovePromocode(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
