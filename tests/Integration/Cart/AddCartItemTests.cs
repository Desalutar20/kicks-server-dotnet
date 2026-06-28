using Application.Carts.Types;
using Application.ProductSkus.Types;
using Presentation.Shared.Dto;

namespace Integration.Cart;

public sealed class AddCartItemTests(ApiFactory factory) : TestApp(factory)
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
        cart.Data.Items[0].Id.Should().Be(body.Data[0].Id);
        cart.Data.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public async ValueTask Should_IncreaseQuantity_When_ProductAlreadyExistsInCart()
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

        var addCartItemResponse2 = await AddCartItem(body.Data[0].Id, sessionCookie, ct);
        addCartItemResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        var getCartResponse = await GetCart(sessionCookie, ct);
        getCartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cart = await getCartResponse.Content.ReadFromJsonAsync<ApiResponse<CartResponse>>(ct);

        cart.Should().NotBeNull();
        cart.Data.Items.Count.Should().Be(1);
        cart.Data.Items[0].Id.Should().Be(body.Data[0].Id);
        cart.Data.Items[0].Quantity.Should().Be(10);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_MaxCartItemsLimitExceeded()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var productSkuIds = await GetProductSkuIdsFromDb(
            Domain.Carts.Cart.MaxCartItemsLength + 1,
            ct
        );

        var responses = new List<HttpResponseMessage>();

        foreach (var id in productSkuIds.Take(Domain.Carts.Cart.MaxCartItemsLength))
        {
            var response = await AddCartItem(id, sessionCookie, ct);

            responses.Add(response);
        }

        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

        var lastResponse = await AddCartItem(productSkuIds.Last(), sessionCookie, ct);

        lastResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductSkuNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var updateProductSkuResponse = await AddCartItem(Guid.NewGuid(), sessionCookie, ct);

        updateProductSkuResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await AddCartItem(Guid.NewGuid(), null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
