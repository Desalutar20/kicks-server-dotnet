using Domain.Orders;
using Microsoft.AspNetCore.Mvc;
using Presentation.Cart.Endpoints;
using Presentation.Orders.Endpoints;
using Presentation.ProductSkus.Dto;
using Presentation.Shared.Dto;
using DeliveryOptionDto = Presentation.DeliveryOptions.Dto.DeliveryOptionDto;

namespace Integration.Orders;

public sealed class CreateOrderTests(ApiFactory factory) : TestApp(factory)
{
    [Fact]
    public async ValueTask Should_ReturnCreated_When_RequestIsValid()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetProductSkus(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuDto>>(ct);
        body.Should().NotBeNull();

        var addCartItemResponse = await AddCartItem(body.Data[0].Id, sessionCookie, ct);
        addCartItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDeliveryOptionsResponse = await GetDeliveryOptions(sessionCookie, ct);
        getDeliveryOptionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getDeliveryOptionsResponseBody =
            await getDeliveryOptionsResponse.Content.ReadFromJsonAsync<
                ApiResponse<IReadOnlyList<DeliveryOptionDto>>
            >(ct);

        getDeliveryOptionsResponseBody.Should().NotBeNull();

        var notFreeDeliveryOption = getDeliveryOptionsResponseBody.Data.First(x => x.Price > 0);

        var createOrderRequest = TestData.CreateOrderRequest(notFreeDeliveryOption.Id.ToString());

        var createOrderResponse = await CreateOrder(createOrderRequest, sessionCookie, ct);
        createOrderResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createOrderResponseBody = await createOrderResponse.Content.ReadFromJsonAsync<
            ApiResponse<Guid>
        >(ct);

        createOrderResponseBody.Should().NotBeNull();

        var orderFromDb = await GetOrderFromDbById(new OrderId(createOrderResponseBody.Data), ct);

        orderFromDb.Should().NotBeNull();
        orderFromDb.OrderItems.Count.Should().Be(1);

        orderFromDb.OrderItems[0].ProductSku.Id.Value.Should().Be(body.Data[0].Id);
        orderFromDb.OrderItems[0].Quantity.Value.Should().Be(1);

        orderFromDb.DeliveryPrice.Dollars.Should().Be(notFreeDeliveryOption.Price);

        orderFromDb
            .Total.Dollars.Should()
            .Be((body.Data[0].SalePrice ?? body.Data[0].Price) * 1 + notFreeDeliveryOption.Price);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async ValueTask Should_ReturnBadRequest_When_RequestIsInvalid(
        string field,
        CreateOrderRequest invalidRequest
    )
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await CreateOrder(invalidRequest, sessionCookie, ct);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(ct);

        error!.Status.Should().Be(400);
        error.Errors[field].Should().NotBeNull();
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_OrderWithStatusPendingAlreadyExists()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetProductSkus(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuDto>>(ct);
        body.Should().NotBeNull();

        var addCartItemResponse = await AddCartItem(body.Data[0].Id, sessionCookie, ct);
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

        var createOrderResponse2 = await CreateOrder(createOrderRequest, sessionCookie, ct);
        createOrderResponse2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_MaxCancelledOrdersPerDayReached()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetProductSkus(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuDto>>(ct);
        body.Should().NotBeNull();

        var addCartItemResponse = await AddCartItem(body.Data[0].Id, sessionCookie, ct);
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

        for (var i = 0; i < Config.Application.MaxCancelledOrdersPerDay; i++)
        {
            var createOrderResponse = await CreateOrder(createOrderRequest, sessionCookie, ct);
            createOrderResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var createOrderResponseBody = await createOrderResponse.Content.ReadFromJsonAsync<
                ApiResponse<Guid>
            >(ct);

            createOrderResponseBody.Should().NotBeNull();

            await CancelOrderInDbById(new OrderId(createOrderResponseBody.Data), ct);
        }

        var createOrderResponse2 = await CreateOrder(createOrderRequest, sessionCookie, ct);
        createOrderResponse2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnBadRequest_When_DeliveryOptionNotFound()
    {
        var ct = TestContext.Current.CancellationToken;

        var request = TestData.SignUpRequest();
        var sessionCookie = await CreateAndSignIn(request, ct);

        var response = await GetProductSkus(null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiCursorResponse<ProductSkuDto>>(ct);
        body.Should().NotBeNull();

        var addCartItemResponse = await AddCartItem(body.Data[0].Id, sessionCookie, ct);
        addCartItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createOrderRequest = TestData.CreateOrderRequest(Guid.NewGuid().ToString());

        var createOrderResponse = await CreateOrder(createOrderRequest, sessionCookie, ct);
        createOrderResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async ValueTask Should_ReturnUnauthorized_When_UserNotSignedIn()
    {
        var ct = TestContext.Current.CancellationToken;
        var request = TestData.CreateOrderRequest("");

        var response = await CreateOrder(request, null, ct);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public static TheoryData<string, CreateOrderRequest> InvalidRequests()
    {
        var createOrderRequest = TestData.CreateOrderRequest(Guid.NewGuid().ToString());

        return
        [
            ("email", createOrderRequest with { Email = "" }),
            ("email", createOrderRequest with { Email = "   " }),
            ("email", createOrderRequest with { Email = "invalid email" }),
            ("phoneNumber", createOrderRequest with { PhoneNumber = "" }),
            ("phoneNumber", createOrderRequest with { PhoneNumber = "   " }),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with { City = "" },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with { City = "   " },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with
                    {
                        City = TestData.String(OrderAddress.MaxCityLength + 1),
                    },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with { Street = "" },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with { Street = "   " },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with
                    {
                        Street = TestData.String(OrderAddress.MaxStreetLength + 1),
                    },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with { Home = "" },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with { Home = "   " },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with
                    {
                        Home = TestData.String(OrderAddress.MaxHomeLength + 1),
                    },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with { Apartment = "" },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with { Apartment = "   " },
                }
            ),
            (
                "billingAddress",
                createOrderRequest with
                {
                    BillingAddress = createOrderRequest.BillingAddress! with
                    {
                        Apartment = TestData.String(OrderAddress.MaxApartmentLength + 1),
                    },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with { City = "" },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with { City = "   " },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with
                    {
                        City = TestData.String(OrderAddress.MaxCityLength + 1),
                    },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with { Street = "" },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with { Street = "   " },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with
                    {
                        Street = TestData.String(OrderAddress.MaxStreetLength + 1),
                    },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with { Home = "" },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with { Home = "   " },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with
                    {
                        Home = TestData.String(OrderAddress.MaxHomeLength + 1),
                    },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with { Apartment = "" },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with { Apartment = "   " },
                }
            ),
            (
                "deliveryAddress",
                createOrderRequest with
                {
                    DeliveryAddress = createOrderRequest.DeliveryAddress with
                    {
                        Apartment = TestData.String(OrderAddress.MaxApartmentLength + 1),
                    },
                }
            ),
        ];
    }
}
