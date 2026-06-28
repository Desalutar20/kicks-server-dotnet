using Application.Admin.DeliveryOptions.Types;
using Application.Carts.Types;
using Domain.Orders;

namespace Application.Orders.Types;

public sealed record OrderResponse
{
    public Guid Id { get; init; }
    public List<OrderItemResponse> Items { get; init; } = [];
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public OrderAddressResponse? BillingAddress { get; init; }
    public required OrderAddressResponse DeliveryAddress { get; init; }
    public OrderStatus Status { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public decimal DeliveryPrice { get; init; }
    public decimal TotalPrice { get; init; }
    public PromocodeResponse? Promocode { get; init; }
    public required DeliveryOptionResponse DeliveryOption { get; init; }
}
