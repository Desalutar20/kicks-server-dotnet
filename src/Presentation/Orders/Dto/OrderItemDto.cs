using Presentation.ProductSkus.Dto;

namespace Presentation.Orders.Dto;

public sealed record OrderItemDto(ProductSkuDto ProductSku, int Quantity, decimal Price);
