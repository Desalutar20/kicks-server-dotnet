namespace Application.ProductSkus.Types;

public sealed record ProductSkuVariantColor(string Color, Guid ProductSkuId, bool InStock);
