using Domain.Abstractions;

namespace Domain.Product.Exceptions;

public sealed class BrandDoesNotExistsException(Exception innerException)
    : AppException("Brand doesn't exist.", innerException);
