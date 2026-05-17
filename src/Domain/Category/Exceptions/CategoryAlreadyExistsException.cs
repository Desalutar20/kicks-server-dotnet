using Domain.Abstractions;

namespace Domain.Category.Exceptions;

public sealed class CategoryAlreadyExistsException(Exception innerException)
    : AppException("Category already exists.", innerException);
