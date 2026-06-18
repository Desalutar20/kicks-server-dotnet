using Domain.Abstractions;

namespace Domain.Orders.Exceptions;

public sealed class PendingOrderAlreadyExistsException(Exception innerException)
    : AppException("Pending order already exists.", innerException);
