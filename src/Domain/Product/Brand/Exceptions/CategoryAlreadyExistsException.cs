using Domain.Abstractions;

namespace Domain.Product.Brand.Exceptions;

public sealed class BrandAlreadyExistsException(Exception innerException)
    : AppException("Brand already exists.", innerException);
