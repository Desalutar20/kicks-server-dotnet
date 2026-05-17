using Domain.Abstractions;

namespace Domain.Product.ProductSku.Exceptions;

public class ProductSkuSkuAlreadyExistsException(Exception inner)
    : AppException("Product sku with same sku already exists", inner);
