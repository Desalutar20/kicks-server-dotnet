using Domain.Abstractions;

namespace Domain.Product.Exceptions;

public sealed class CategoryDoesNotExistsException(Exception innerException)
    : AppException("Category doesn't exist.", innerException);
