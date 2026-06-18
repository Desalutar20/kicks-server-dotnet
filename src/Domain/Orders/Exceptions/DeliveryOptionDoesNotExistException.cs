using Domain.Abstractions;

namespace Domain.Orders.Exceptions;

public sealed class DeliveryOptionDoesNotExistsException(Exception innerException)
    : AppException("Delivery option doesn't exists.", innerException);
