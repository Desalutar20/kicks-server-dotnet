namespace Presentation.Cart.Endpoints;

internal static partial class OrderEndpoints
{
    public static void MapCartV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/cart").WithTags("Cart");

        group
            .GetCartV1()
            .AddCartItemV1()
            .UpdateCartItemQuantityV1()
            .RemoveCartItemV1()
            .ClearCartV1()
            .ApplyPromocodeV1()
            .RemovePromocodeV1();
    }
}
