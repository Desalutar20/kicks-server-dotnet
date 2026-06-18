using Domain.Abstractions;

namespace Domain.Products.ProductSkus.Exceptions;

public sealed class ProductDoesNotExistsException(Exception innerException)
    : AppException("Product doesn't exist.", innerException);
