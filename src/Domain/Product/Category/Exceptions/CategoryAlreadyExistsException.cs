using Domain.Abstractions;

namespace Domain.Product.Category.Exceptions;

public sealed class CategoryAlreadyExistsException(Exception innerException)
    : AppException("Category already exists.", innerException);
