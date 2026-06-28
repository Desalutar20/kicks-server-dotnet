using Application.Admin.DeliveryOptions.Types;
using Application.Orders.Constants;
using Application.Orders.Types;
using Application.ProductSkus.Types;
using Domain.Orders;
using Microsoft.AspNetCore.Mvc;
using Presentation.Orders.Endpoints;
using Presentation.Shared.Dto;

namespace Integration.Orders;

public sealed class GetOrdersTests(ApiFactory factory) : TestApp(factory)
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

        var getOrdersResponse = await GetOrders(null, sessionCookie, ct);
        getOrdersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var ordersResponseBody = await getOrdersResponse.Content.ReadFromJsonAsync<
            ApiCursorResponse<OrderResponse>
        >(ct);

        ordersResponseBody.Should().NotBeNull();

        ordersResponseBody.Data.Count.Should().Be(1);

        var order = ordersResponseBody.Data.Single();

        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);

        order.Items.Count.Should().Be(1);

        order.Items[0].Quantity.Should().Be(1);

        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        GetOrdersRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetOrders(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await GetOrders(null, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public static TheoryData<string, GetOrdersRequest> InvalidRequests()
    {
        var request = new GetOrdersRequest(null, null, null);

        return
        [
            ("limit", request with { Limit = 0 }),
            ("limit", request with { Limit = OrderConstants.GetOrdersMaxLimit + 1 }),
            (
                "prevCursor",
                request with
                {
                    PrevCursor = TestData.String(OrderConstants.GetOrdersCursorMaxLength + 1),
                }
            ),
            (
                "nextCursor",
                request with
                {
                    NextCursor = TestData.String(OrderConstants.GetOrdersCursorMaxLength + 1),
                }
            ),
            ("prevCursor", request with { PrevCursor = "prev", NextCursor = "next" }),
        ];
    }
}
