using Domain.Abstractions;

namespace Domain.Brands.Exceptions;

public sealed class BrandAlreadyExistsException(Exception innerException)
    : AppException("Brand already exists.", innerException);
