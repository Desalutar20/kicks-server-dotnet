using Presentation.ProductSkus.Dto;

namespace Presentation.Cart.Dto;

public sealed record CartItemDto(ProductSkuDto ProductSku, int Quantity);
