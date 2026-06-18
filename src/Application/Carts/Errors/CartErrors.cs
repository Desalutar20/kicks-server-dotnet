namespace Application.Carts.Errors;

internal static class CartErrors
{
    public static Error CartNotFound => Error.Failure("Cart not found.");
    public static Error InvalidPromocode => Error.Failure("Invalid promocode.");
    public static Error PromocodeAlreadyUsed => Error.Failure("Promocode has already been used.");
    public static Error ProductSkuNotFound => Error.Failure("Product SKU not found.");
    public static Error DeliveryOptionNotFound => Error.Failure("Delivery option not found.");
}
