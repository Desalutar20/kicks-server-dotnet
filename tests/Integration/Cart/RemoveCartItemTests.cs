using Application.Carts.Types;
using Application.ProductSkus.Types;
using Presentation.Shared.Dto;

namespace Integration.Cart;

public sealed class RemoveCartItemTests(ApiFactory factory) : TestApp(factory)
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

        var getCartResponse = await GetCart(sessionCookie, ct);
        getCartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cart = await getCartResponse.Content.ReadFromJsonAsync<ApiResponse<CartResponse>>(ct);

        cart.Should().NotBeNull();
        cart.Data.Items.Count.Should().Be(1);

        var removeCartItemResponse = await RemoveCartItem(body.Data[0].Id, sessionCookie, ct);
        removeCartItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getCartAfterRemovingCartItemResponse = await GetCart(sessionCookie, ct);
        getCartAfterRemovingCartItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cartAfterRemovingCartItem =
            await getCartAfterRemovingCartItemResponse.Content.ReadFromJsonAsync<
                ApiResponse<CartResponse>
            >(ct);

        cartAfterRemovingCartItem.Should().NotBeNull();
        cartAfterRemovingCartItem.Data.Items.Count.Should().Be(0);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await RemoveCartItem(Guid.NewGuid(), null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
