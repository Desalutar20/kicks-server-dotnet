using Domain.Abstractions;

namespace Domain.Products.Exceptions;

public sealed class CategoryDoesNotExistsException(Exception innerException)
    : AppException("Category doesn't exist.", innerException);
