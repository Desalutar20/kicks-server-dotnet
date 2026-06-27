namespace Application.Orders.Errors;

public static class OrderErrors
{
    public static Error DeliveryOptionNotFound => Error.Failure("Delivery option not found.");
    public static Error OrderCreationLimitExceeded =>
        Error.Failure("Order creation limit exceeded.");

    public static Error OrderExpired => Error.Failure("Order has expired.");

    public static Error AlreadyHasPendingError =>
        Error.Failure(
            "You already have a pending order. Complete or cancel it before creating a new one."
        );

    public static Error OrderNotFound => Error.Failure("Order not found.");
}
