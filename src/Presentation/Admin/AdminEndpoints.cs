using Presentation.Admin.Brands.Endpoints;
using Presentation.Admin.Categories.Endpoints;
using Presentation.Admin.DeliveryOptions.Endpoints;
using Presentation.Admin.Products.Endpoints;
using Presentation.Admin.Promocodes.Endpoints;
using Presentation.Admin.Users.Endpoints;

namespace Presentation.Admin;

public static class AdminEndpoints
{
    public static void MapAdminV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/admin");

        group.MapAdminUsersV1();
        group.MapAdminBrandsV1();
        group.MapAdminCategoriesV1();
        group.MapAdminProductsV1();
        group.MapAdminPromocodesV1();
        group.MapAdminDeliveryOptionsV1();
    }
}
