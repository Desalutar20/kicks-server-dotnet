using Domain.Abstractions;

namespace Domain.Products.ProductSkus.Exceptions;

public class ProductSkuSkuAlreadyExistsException(Exception inner)
    : AppException("Product sku with same sku already exists", inner);
