using Domain.Abstractions;

namespace Domain.Carts.Exceptions;

public sealed class ProductSkuDoesNotExistsException(Exception innerException)
    : AppException("Product sku doesn't exists.", innerException);
