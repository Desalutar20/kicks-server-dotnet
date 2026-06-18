using Domain.Orders;
using Presentation.Cart.Endpoints;
using Presentation.Orders.Dto;
using Presentation.ProductSkus.Dto;
using Presentation.Shared.Dto;
using DeliveryOptionDto = Presentation.DeliveryOptions.Dto.DeliveryOptionDto;

namespace Integration.Orders;

public sealed class GetOrderTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnOk_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetProductSkus(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuDto>>(ct);
        body.Should().NotBeNull();

        const int quantity = 5;

        var addCartItemResponse = await AddCartItem(
            new AddCartItemRequest(body.Data[0].Id.ToString(), quantity),
            sessionCookie,
            ct
        );

        addCartItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDeliveryOptionsResponse = await GetDeliveryOptions(sessionCookie, ct);
        getDeliveryOptionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDeliveryOptionsResponseBody =
            await getDeliveryOptionsResponse.Content.ReadFromJsonAsync<
                ApiResponse<IReadOnlyList<DeliveryOptionDto>>
            >(ct);

        getDeliveryOptionsResponseBody.Should().NotBeNull();

        var createOrderRequest = TestData.CreateOrderRequest(
            getDeliveryOptionsResponseBody.Data[0].Id.ToString()
        );
        var createOrderResponse = await CreateOrder(createOrderRequest, sessionCookie, ct);
        createOrderResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createOrderResponseBody = await createOrderResponse.Content.ReadFromJsonAsync<
            ApiResponse<Guid>
        >(ct);

        createOrderResponseBody.Should().NotBeNull();

        var getOrdersResponse = await GetOrder(createOrderResponseBody.Data, sessionCookie, ct);
        getOrdersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var orderResponseBody = await getOrdersResponse.Content.ReadFromJsonAsync<
            ApiResponse<OrderDto>
        >(ct);

        orderResponseBody.Should().NotBeNull();

        var order = orderResponseBody.Data;

        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);

        order.Items.Count.Should().Be(1);

        order.Items[0].ProductSku.Id.Should().Be(body.Data[0].Id);
        order.Items[0].Quantity.Should().Be(quantity);

        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await GetOrder(Guid.NewGuid(), null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
