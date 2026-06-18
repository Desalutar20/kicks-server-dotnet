using Domain.Orders;
using Presentation.Cart.Dto;
using Presentation.ProductSkus.Dto;

namespace Presentation.Orders.Dto;

public sealed record OrderDto(
    Guid Id,
    List<OrderItemDto> Items,
    PromocodeDto? Promocode,
    string Email,
    string PhoneNumber,
    OrderAddressDto? BillingAddress,
    OrderAddressDto DeliveryAddress,
    OrderStatus Status,
    DateTimeOffset ExpiresAt,
    decimal DeliveryPrice,
    decimal TotalPrice,
    DeliveryOptionDto DeliveryOption
);

internal static class OrderDtoMapper
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto(
            order.Id,
            order
                .OrderItems.Select(item => new OrderItemDto(
                    item.ProductSku.ToDto(),
                    item.Quantity,
                    item.Price.Dollars
                ))
                .ToList(),
            order.Promocode?.ToDto(),
            order.Email.Value,
            order.PhoneNumber.Value,
            order.BillingAddress?.ToDto(),
            order.DeliveryAddress.ToDto(),
            order.Status,
            order.ExpiresAt,
            order.DeliveryPrice.Dollars,
            order.Total.Dollars,
            new DeliveryOptionDto(
                order.DeliveryOption.Title.Value,
                order.DeliveryOption.Description.Value
            )
        );
    }
}
