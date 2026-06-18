using Microsoft.AspNetCore.Mvc;
using Presentation.Cart.Dto;
using Presentation.Cart.Endpoints;
using Presentation.ProductSkus.Dto;
using Presentation.Shared.Dto;

namespace Integration.Cart;

public class AddCartItemTests(ApiFactory factory) : TestApp(factory)
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
        cart.Data.Items[0].ProductSku.Id.Should().Be(body.Data[0].Id);
        cart.Data.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public async ValueTask Should_IncreaseQuantity_When_ProductAlreadyExistsInCart()
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

        var addCartItemResponse2 = await AddCartItem(
            new AddCartItemRequest(body.Data[0].Id.ToString(), 5),
            sessionCookie,
            ct
        );
        addCartItemResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

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
        AddCartItemRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var response = await AddCartItem(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_MaxCartItemsLimitExceeded()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var productSkuIds = await GetProductSkuIdsFromDb(
            Domain.Carts.Cart.MaxCartItemsLength + 1,
            ct
        );

        var responses = new List<HttpResponseMessage>();

        foreach (var id in productSkuIds.Take(Domain.Carts.Cart.MaxCartItemsLength))
        {
            var response = await AddCartItem(
                new AddCartItemRequest(id.ToString(), 5),
                sessionCookie,
                ct
            );

            responses.Add(response);
        }

        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

        var lastResponse = await AddCartItem(
            new AddCartItemRequest(productSkuIds.Last().ToString(), 5),
            sessionCookie,
            ct
        );

        lastResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_ProductSkuNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct, Role.Admin);

        var addCartItemRequest = new AddCartItemRequest(Guid.NewGuid().ToString(), 5);
        var updateProductSkuResponse = await AddCartItem(addCartItemRequest, sessionCookie, ct);

        updateProductSkuResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = new AddCartItemRequest("", 5);

        var response = await AddCartItem(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public static TheoryData<string, AddCartItemRequest> InvalidRequests() =>
        [
            ("productSkuId", new AddCartItemRequest("", 5)),
            ("productSkuId", new AddCartItemRequest("   ", 5)),
            ("productSkuId", new AddCartItemRequest("not guid", 5)),
            ("quantity", new AddCartItemRequest("3fc20a06-f07d-4427-9731-ee8899636a8c", 0)),
            ("quantity", new AddCartItemRequest("3fc20a06-f07d-4427-9731-ee8899636a8c", -1)),
        ];
}
