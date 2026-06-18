using Domain.Orders;

namespace Presentation.Orders.Dto;

public sealed record OrderAddressDto(string City, string Street, string Home, string Apartment);

internal static class OrderAddressDtoMapper
{
    public static OrderAddressDto ToDto(this OrderAddress address) =>
        new(address.City, address.Street, address.Home, address.Apartment);
}
