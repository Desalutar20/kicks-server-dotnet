using Domain.DeliveryOptions;

namespace Application.Admin.DeliveryOptions.Errors;

internal static class AdminDeliveryOptionsErrors
{
    public static Result DeliveryOptionNotFound(DeliveryOptionId deliveryOptionId) =>
        Error.Failure($"Delivery option with id '{deliveryOptionId}' doesn't exist");

    public static Error DeliveryOptionAlreadyExists(DeliveryOptionTitle title) =>
        Error.Failure($"Delivery option with name '{title}' already exists");
}
