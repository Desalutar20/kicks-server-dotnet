namespace Presentation.Admin.Brands.Endpoints;

internal static partial class AdminPromocodesEndpoints
{
    public static void MapAdminBrandsV1(this IEndpointRouteBuilder router)
    {
        var group = router.MapGroup("/brands").WithTags("Admin brands");

        group.GetBrandsV1().CreateBrandV1().UpdateBrandV1().DeleteBrandV1();
    }
}
