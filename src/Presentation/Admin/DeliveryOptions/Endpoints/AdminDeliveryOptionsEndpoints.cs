namespace Presentation.Admin.DeliveryOptions.Endpoints;

internal static partial class AdminDeliveryOptionsEndpoints
{
    public static void MapAdminDeliveryOptionsV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/delivery-options").WithTags("Admin delivery options");

        group
            .GetDeliveryOptionsV1()
            .CreateDeliveryOptionV1()
            .UpdateDeliveryOptionV1()
            .DeleteDeliveryOptionV1();
    }
}
