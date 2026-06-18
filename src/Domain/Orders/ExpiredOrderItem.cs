using Domain.Products.ProductSkus;
using Domain.Promocodes;
using Domain.Shared.ValueObjects;

namespace Domain.Orders;

public sealed record ExpiredOrderItem(
    OrderId Id,
    PromocodeId? PromocodeId,
    IReadOnlyList<(ProductSkuId Id, PositiveInt Quantity)> Items
);

public sealed record ExpiredOrderDbRow(Guid Id, Guid? PromocodeId);

public sealed record ExpiredOrderItemDbRow(Guid ProductSkuId, int Quantity);
