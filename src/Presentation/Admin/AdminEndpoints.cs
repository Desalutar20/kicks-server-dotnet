using Presentation.Admin.Brands.Endpoints;
using Presentation.Admin.Categories.Endpoints;
using Presentation.Admin.Users.Endpoints;

namespace Presentation.Admin;

public static class AdminEndpoints
{
    public static void MapAdminV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/admin");

        group.MapAdminUsersV1();
        group.MapBrandsV1();
        group.MapCategoriesV1();
    }
}
