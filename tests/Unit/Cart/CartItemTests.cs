using Domain.Carts;
using Domain.Products.ProductSkus;
using Domain.Shared;
using Domain.Shared.ValueObjects;
using FluentAssertions;

namespace Unit.Cart;

public class CartItemTests
{
    [Fact]
    public void UpdateQuantity_Should_Change_Quantity()
    {
        var item = new CartItem(PositiveInt.Create(3).Value, new ProductSkuId(Guid.NewGuid()));

        item.UpdateQuantity(PositiveInt.Create(10).Value);

        item.Quantity.Value.Should().Be(10);
    }
}
