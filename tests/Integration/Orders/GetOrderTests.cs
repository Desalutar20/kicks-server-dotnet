using Application.Admin.DeliveryOptions.Types;
using Application.Orders.Types;
using Application.ProductSkus.Types;
using Domain.Orders;
using Presentation.Shared.Dto;

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

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuResponse>>(
            ct
        );
        body.Should().NotBeNull();

        var addCartItemResponse = await AddCartItem(body.Data[0].Id, sessionCookie, ct);

        addCartItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDeliveryOptionsResponse = await GetDeliveryOptions(sessionCookie, ct);
        getDeliveryOptionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDeliveryOptionsResponseBody =
            await getDeliveryOptionsResponse.Content.ReadFromJsonAsync<
                ApiResponse<IReadOnlyList<DeliveryOptionResponse>>
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
            ApiResponse<OrderResponse>
        >(ct);

        orderResponseBody.Should().NotBeNull();

        var order = orderResponseBody.Data;

        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);

        order.Items.Count.Should().Be(1);

        order.Items[0].Id.Should().Be(body.Data[0].Id);
        order.Items[0].Quantity.Should().Be(1);

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
