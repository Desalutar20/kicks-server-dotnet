namespace Presentation.ProductSkus.Endpoints;

internal static partial class CartEndpoints
{
    public static void MapProductSkusV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/product-skus").WithTags("Product skus");

        group.GetProductSkusFiltersV1().GetProductSkusV1().GetProductSkuV1();
    }
}
