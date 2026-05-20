namespace Presentation.Admin.Products.ProductSkus.Endpoints;

internal static partial class AdminProductSkusEndpoints
{
    public static void MapAdminProductSkusV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("").WithTags("Admin product skus");

        group
            .GetAdminProductSkusV1()
            .GetAdminProductSkuV1()
            .CreateProductSkuV1()
            .UpdateProductSkuV1()
            .DeleteProductSkuV1()
            .DeleteProductSkuImageV1();
    }
}
