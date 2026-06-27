using Domain.Shared.ValueObjects;

namespace Application.Admin.DeliveryOptions.Constants;

public static class DeliveryOptionsConstants
{
    public static NonEmptyString CacheKey = NonEmptyString.Create("delivery-options").Value;
}
