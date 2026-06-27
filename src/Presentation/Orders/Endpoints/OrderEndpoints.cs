namespace Presentation.Orders.Endpoints;

internal static partial class OrderEndpoints
{
    public static void MapOrdersV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/orders").WithTags("Orders");

        group.GetOrdersV1().GetOrderV1().CreateOrderV1().CreateOrderPaymentV1();
    }
}
