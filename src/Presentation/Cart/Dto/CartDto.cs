using Presentation.ProductSkus.Dto;

namespace Presentation.Cart.Dto;

public sealed record CartDto(
    List<CartItemDto> Items,
    PromocodeDto? Promocode,
    decimal TotalPrice,
    decimal? PriceWithDiscount
);

internal static class CartDtoMapper
{
    public static CartDto ToDto(this Domain.Carts.Cart cart)
    {
        var (totalPrice, priceWithDiscount) = cart.TotalPrice;

        return new CartDto(
            cart.CartItems.Select(item => new CartItemDto(
                    item.ProductSku.ToDto(),
                    item.FinalQuantity
                ))
                .ToList(),
            cart.Promocode is not null && cart.Promocode.IsValid ? cart.Promocode.ToDto() : null,
            totalPrice.Dollars,
            priceWithDiscount?.Dollars
        );
    }
}
