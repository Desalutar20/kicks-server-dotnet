using Presentation.Admin.Products.ProductSkus.Endpoints;

namespace Presentation.Admin.Products.Endpoints;

internal static partial class AdminProductSkusEndpoints
{
    public static void MapProductsV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/products").WithTags("Admin products");

        group
            .GetProductFiltersV1()
            .GetProductsV1()
            .CreateProductV1()
            .UpdateProductV1()
            .ToggleProductIsDeletedV1();

        group.MapProductSkusV1();
    }
}
