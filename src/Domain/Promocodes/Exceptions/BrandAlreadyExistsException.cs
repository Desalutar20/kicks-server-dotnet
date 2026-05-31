using Domain.Abstractions;

namespace Domain.Promocodes.Exceptions;

public sealed class PromocodeAlreadyExistsException(Exception innerException)
    : AppException("Promocode already exists.", innerException);
