using Domain.Abstractions;

namespace Domain.Orders.Exceptions;

public sealed class PromocodeAlreadyUsedException(Exception innerException)
    : AppException("Promocode already used.", innerException);
