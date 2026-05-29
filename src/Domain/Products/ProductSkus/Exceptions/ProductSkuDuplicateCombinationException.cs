using Domain.Abstractions;

namespace Domain.Products.ProductSkus.Exceptions;

public class ProductSkuDuplicateCombinationException(Exception innerException)
    : AppException(
        "Product sku with same productId, size and color already exists ",
        innerException
    );
