using Domain.Products.ProductSkus;
using Domain.Shared.ValueObjects;

namespace Domain.Orders;

public sealed class OrderItem(PositiveInt quantity, Money price, ProductSkuId productSkuId)
{
    public PositiveInt Quantity { get; private set; } = quantity;
    public Money Price { get; private set; } = price;

    public ProductSkuId ProductSkuId { get; private set; } = productSkuId;
    public ProductSku ProductSku { get; private set; } = null!;
}
