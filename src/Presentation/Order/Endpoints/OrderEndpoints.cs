namespace Presentation.Order.Endpoints;

internal static partial class OrderEndpoints
{
    public static void MapOrdersV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/orders").WithTags("Orders");

        group.CreateOrderV1();
    }
}
