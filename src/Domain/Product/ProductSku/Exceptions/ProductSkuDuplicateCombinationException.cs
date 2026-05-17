using Domain.Abstractions;

namespace Domain.Product.ProductSku.Exceptions;

public class ProductSkuDuplicateCombinationException(Exception innerException)
    : AppException(
        "Product sku with same productId, size and color already exists ",
        innerException
    );
