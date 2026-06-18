namespace Presentation.Admin.DeliveryOptions.Endpoints;

internal static partial class DeliveryOptionsEndpoints
{
    public static void MapAdminDeliveryOptionsV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/delivery-options").WithTags("Admin delivery options");

        group
            .GetAdminDeliveryOptionsV1()
            .CreateDeliveryOptionV1()
            .UpdateDeliveryOptionV1()
            .DeleteDeliveryOptionV1();
    }
}
