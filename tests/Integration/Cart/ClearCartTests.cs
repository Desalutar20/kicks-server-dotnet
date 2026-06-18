using Presentation.Cart.Dto;
using Presentation.Cart.Endpoints;
using Presentation.ProductSkus.Dto;
using Presentation.Shared.Dto;

namespace Integration.Cart;

public class ClearCartTests(ApiFactory factory) : TestApp(factory)
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

        var getCartResponse = await GetCart(sessionCookie, ct);
        getCartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cart = await getCartResponse.Content.ReadFromJsonAsync<ApiResponse<CartDto>>(ct);

        cart.Should().NotBeNull();
        cart.Data.Items.Count.Should().Be(1);

        var clearCartResponse = await ClearCart(sessionCookie, ct);
        clearCartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getCartAfterClearingResponse = await GetCart(sessionCookie, ct);
        getCartAfterClearingResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cartAfterClearing = await getCartAfterClearingResponse.Content.ReadFromJsonAsync<
            ApiResponse<CartDto>
        >(ct);

        cartAfterClearing.Should().NotBeNull();
        cartAfterClearing.Data.Items.Count.Should().Be(0);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await ClearCart(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
