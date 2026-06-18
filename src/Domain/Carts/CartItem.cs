using Domain.Products.ProductSkus;
using Domain.Shared.ValueObjects;

namespace Domain.Carts;

public sealed class CartItem
{
    private CartItem() { }

    public CartItem(PositiveInt quantity, ProductSkuId productSkuId)
    {
        Quantity = quantity;
        ProductSkuId = productSkuId;
    }

    public PositiveInt Quantity { get; private set; } = null!;
    public ProductSkuId ProductSkuId { get; private set; } = null!;
    public ProductSku ProductSku { get; private set; } = null!;

    public PositiveInt FinalQuantity =>
        Quantity > ProductSku.Quantity ? PositiveInt.Create(ProductSku.Quantity).Value : Quantity;

    public void UpdateQuantity(PositiveInt quantity) => Quantity = quantity;
}
