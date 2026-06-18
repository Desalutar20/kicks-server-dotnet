using Microsoft.AspNetCore.Mvc;
using Presentation.Cart.Dto;
using Presentation.Cart.Endpoints;
using Presentation.ProductSkus.Dto;
using Presentation.Shared.Dto;

namespace Integration.Cart;

public class UpdateCartItemQuantityTests(ApiFactory factory) : TestApp(factory)
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

        var updateCartItemQuantityResponse = await UpdateCartItemQuantity(
            body.Data[0].Id,
            new UpdateCartItemQuantityRequest(10),
            sessionCookie,
            ct
        );
        updateCartItemQuantityResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getCartResponse = await GetCart(sessionCookie, ct);
        getCartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cart = await getCartResponse.Content.ReadFromJsonAsync<ApiResponse<CartDto>>(ct);

        cart.Should().NotBeNull();
        cart.Data.Items.Count.Should().Be(1);
        cart.Data.Items[0].ProductSku.Id.Should().Be(body.Data[0].Id);
        cart.Data.Items[0].Quantity.Should().Be(10);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        UpdateCartItemQuantityRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await UpdateCartItemQuantity(
            Guid.NewGuid(),
            invalidRequest,
            sessionCookie,
            ct
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductSkuNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var updateCartItemQuantityResponse = new UpdateCartItemQuantityRequest(5);
        var updateProductSkuResponse = await UpdateCartItemQuantity(
            Guid.NewGuid(),
            updateCartItemQuantityResponse,
            sessionCookie,
            ct
        );

        updateProductSkuResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new UpdateCartItemQuantityRequest(5);

        var response = await UpdateCartItemQuantity(Guid.NewGuid(), request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public static TheoryData<string, UpdateCartItemQuantityRequest> InvalidRequests() =>
        [
            ("quantity", new UpdateCartItemQuantityRequest(0)),
            ("quantity", new UpdateCartItemQuantityRequest(-1)),
        ];
}
