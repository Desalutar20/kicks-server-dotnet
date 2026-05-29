using Domain.Abstractions;

namespace Domain.Categories.Exceptions;

public sealed class CategoryAlreadyExistsException(Exception innerException)
    : AppException("Category already exists.", innerException);
