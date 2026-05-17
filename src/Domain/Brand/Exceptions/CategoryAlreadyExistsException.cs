using Domain.Abstractions;

namespace Domain.Brand.Exceptions;

public sealed class BrandAlreadyExistsException(Exception innerException)
    : AppException("Brand already exists.", innerException);
