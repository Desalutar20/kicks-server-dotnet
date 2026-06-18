namespace Presentation.DeliveryOptions.Endpoints;

internal static partial class DeliveryOptionsEndpoints
{
    public static void MapDeliveryOptionsV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/delivery-options").WithTags("Delivery options");

        group.GetDeliveryOptionsV1();
    }
}
