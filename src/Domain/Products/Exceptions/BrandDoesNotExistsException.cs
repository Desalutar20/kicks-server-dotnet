using Domain.Abstractions;

namespace Domain.Products.Exceptions;

public sealed class BrandDoesNotExistsException(Exception innerException)
    : AppException("Brand doesn't exist.", innerException);
